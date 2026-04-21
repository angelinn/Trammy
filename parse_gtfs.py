import os
import zipfile
import sqlite3
import pandas as pd
import requests

GTFS_URL = "https://gtfs.sofiatraffic.bg/api/v1/static"

DB_PATH = "sofia.db"
ZIP_PATH = "gtfs.zip"
EXTRACT_DIR = "gtfs"


# -----------------------------
# Download GTFS
# -----------------------------
r = requests.get(GTFS_URL)
with open(ZIP_PATH, "wb") as f:
    f.write(r.content)


# -----------------------------
# Extract
# -----------------------------
if os.path.exists(EXTRACT_DIR):
    os.system(f"rm -rf {EXTRACT_DIR}")

with zipfile.ZipFile(ZIP_PATH) as z:
    z.extractall(EXTRACT_DIR)


# -----------------------------
# Create DB
# -----------------------------
if os.path.exists(DB_PATH):
    os.remove(DB_PATH)

conn = sqlite3.connect(DB_PATH)
cur = conn.cursor()


# -----------------------------
# Performance settings (SQLite equivalent of your PRAGMAs)
# -----------------------------
cur.executescript("""
PRAGMA synchronous = OFF;
PRAGMA journal_mode = MEMORY;
PRAGMA temp_store = MEMORY;
PRAGMA cache_size = -64000;
""")


# -----------------------------
# Tables (same naming as your Dart schema)
# -----------------------------
tables = {
    "agency.txt": "agency",
    "calendar_dates.txt": "calendar_dates",
    "routes.txt": "routes",
    "trips.txt": "trips",
    "stops.txt": "stops",
    "stop_times.txt": "stop_times",
    "shapes.txt": "shapes",
    "transfers.txt": "transfers",
    "translations.txt": "translations",
    "fare_attributes.txt": "fare_attributes",
    "feed_info.txt": "feed_info",
    "levels.txt": "levels",
    "pathways.txt": "pathways",
}


# -----------------------------
# Import CSVs
# -----------------------------
def import_table(file_name, table_name):
    path = os.path.join(EXTRACT_DIR, file_name)
    if not os.path.exists(path):
        return

    df = pd.read_csv(path)
    df = df.where(pd.notnull(df), None)

    cols = list(df.columns)
    placeholders = ",".join(["?"] * len(cols))
    sql = f'INSERT OR REPLACE INTO {table_name} ({",".join(cols)}) VALUES ({placeholders})'

    cur.executemany(sql, df.itertuples(index=False, name=None))
    conn.commit()


# -----------------------------
# Schema creation (minimal but identical meaning)
# -----------------------------
schema = """
CREATE TABLE agency(
agency_id TEXT PRIMARY KEY,
agency_name TEXT,
agency_url TEXT,
agency_timezone TEXT,
agency_lang TEXT,
agency_phone TEXT,
agency_email TEXT
);

CREATE TABLE calendar_dates(
service_id TEXT,
date TEXT,
exception_type INTEGER,
PRIMARY KEY(service_id,date)
);

CREATE TABLE routes(
route_id TEXT PRIMARY KEY,
agency_id TEXT,
route_short_name TEXT,
route_long_name TEXT,
route_desc TEXT,
route_type INTEGER,
route_url TEXT,
route_color TEXT,
route_text_color TEXT,
route_sort_order INTEGER,
continuous_pickup INTEGER,
continuous_drop_off INTEGER
);

CREATE TABLE trips(
trip_id TEXT PRIMARY KEY,
route_id TEXT,
service_id TEXT,
trip_headsign TEXT,
trip_short_name TEXT,
direction_id INTEGER,
block_id TEXT,
shape_id TEXT,
wheelchair_accessible INTEGER,
bikes_allowed INTEGER
);

CREATE TABLE stops(
stop_id TEXT PRIMARY KEY,
stop_code TEXT,
stop_name TEXT,
stop_desc TEXT,
stop_lat REAL,
stop_lon REAL,
location_type INTEGER,
parent_station TEXT,
stop_timezone TEXT,
level_id TEXT
);

CREATE TABLE stop_times(
trip_id TEXT,
arrival_time TEXT,
departure_time TEXT,
stop_id TEXT,
stop_sequence INTEGER,
stop_headsign TEXT,
pickup_type INTEGER,
drop_off_type INTEGER,
shape_dist_traveled REAL,
continuous_pickup INTEGER,
continuous_drop_off INTEGER,
timepoint INTEGER,
PRIMARY KEY(trip_id,stop_sequence)
);

CREATE TABLE shapes(
shape_id TEXT,
shape_pt_lat REAL,
shape_pt_lon REAL,
shape_pt_sequence INTEGER,
shape_dist_traveled REAL,
PRIMARY KEY(shape_id,shape_pt_sequence)
);

CREATE TABLE transfers(
from_stop_id TEXT,
to_stop_id TEXT,
from_route_id TEXT,
to_route_id TEXT,
from_trip_id TEXT,
to_trip_id TEXT,
transfer_type INTEGER,
min_transfer_time INTEGER
);

CREATE TABLE translations(
table_name TEXT,
field_name TEXT,
language TEXT,
translation TEXT,
record_id TEXT,
record_sub_id TEXT,
field_value TEXT
);

CREATE TABLE fare_attributes(
fare_id TEXT PRIMARY KEY,
price REAL,
currency_type TEXT,
payment_method INTEGER,
transfers INTEGER,
agency_id TEXT,
transfer_duration INTEGER
);

CREATE TABLE feed_info(
feed_publisher_name TEXT,
feed_publisher_url TEXT,
feed_lang TEXT,
default_lang TEXT,
feed_start_date TEXT,
feed_end_date TEXT,
feed_version TEXT,
feed_contact_email TEXT,
feed_contact_url TEXT
);

CREATE TABLE levels(
level_id TEXT PRIMARY KEY,
level_index REAL,
level_name TEXT
);

CREATE TABLE pathways(
pathway_id TEXT PRIMARY KEY,
from_stop_id TEXT,
to_stop_id TEXT,
pathway_mode INTEGER,
is_bidirectional INTEGER,
length REAL,
traversal_time INTEGER,
stair_count INTEGER,
max_slope REAL,
min_width REAL,
signposted_as TEXT,
reversed_signposted_as TEXT
);
"""

cur.executescript(schema)


# -----------------------------
# Load all tables
# -----------------------------
for file, table in tables.items():
    import_table(file, table)


# -----------------------------
# Indexes (same purpose as your Dart version)
# -----------------------------
indexes = [
    "CREATE INDEX IF NOT EXISTS idx_stop_times_stop_id ON stop_times(stop_id)",
    "CREATE INDEX IF NOT EXISTS idx_stop_times_trip_id ON stop_times(trip_id)",
    "CREATE INDEX IF NOT EXISTS idx_trips_trip_id ON trips(trip_id)",
    "CREATE INDEX IF NOT EXISTS idx_trips_route_id ON trips(route_id)",
    "CREATE INDEX IF NOT EXISTS idx_routes_route_id ON routes(route_id)",
    "CREATE INDEX IF NOT EXISTS idx_stops_stop_code ON stops(stop_code)",
    "CREATE INDEX IF NOT EXISTS idx_trips_service_id ON trips(service_id)",
    "CREATE INDEX IF NOT EXISTS idx_calendar_dates_service_date ON calendar_dates(service_id,date,exception_type)",
]

for i in indexes:
    cur.execute(i)


# -----------------------------
# Derived table (your optimization step)
# -----------------------------
cur.executescript("""
DROP TABLE IF EXISTS stop_route_info;

CREATE TABLE stop_route_info AS
SELECT 
  stops.stop_id,
  CAST(stops.stop_code AS TEXT) AS stop_code,
  stops.stop_name,
  stops.stop_desc,
  stops.stop_lat,
  stops.stop_lon,
  stops.location_type,
  stops.parent_station,
  stops.stop_timezone,
  stops.level_id,
  GROUP_CONCAT(DISTINCT routes.route_type) AS route_types,
  GROUP_CONCAT(DISTINCT routes.route_color) AS route_colors,
  GROUP_CONCAT(DISTINCT routes.route_id) AS route_ids
FROM stops
JOIN stop_times ON stops.stop_id = stop_times.stop_id
JOIN trips ON stop_times.trip_id = trips.trip_id
JOIN routes ON trips.route_id = routes.route_id
WHERE stops.stop_lat IS NOT NULL AND stops.stop_lon IS NOT NULL
GROUP BY stops.stop_code
""")


conn.commit()
conn.close()
