import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:trammy/screens/database_loading_screen.dart';
import 'package:trammy/services/gtfs_task_manager.dart';

class SettingsScreen extends StatefulWidget {
  const SettingsScreen({super.key});

  @override
  State<SettingsScreen> createState() => SettingsScreenState();
}

class SettingsScreenState extends State<SettingsScreen> {
  bool wifiOnly = true;
  bool autoUpdate = true;
  bool updateNotifications = true;
  String lastUpdated = 'Непознато';
  bool loading = true;

  Future<void> loadSettings() async {
    final prefs = await SharedPreferences.getInstance();
    setState(() {
      wifiOnly = prefs.getBool('settings_wifi_only') ?? true;
      autoUpdate = prefs.getBool('settings_auto_update') ?? true;
      updateNotifications = prefs.getBool('settings_update_notifications') ?? true;
      final ts = prefs.getInt('settings_last_updated');
      if (ts != null) {
        final dt = DateTime.fromMillisecondsSinceEpoch(ts);
        lastUpdated =
            '${dt.day.toString().padLeft(2, '0')}.${dt.month.toString().padLeft(2, '0')}.${dt.year}  '
            '${dt.hour.toString().padLeft(2, '0')}:${dt.minute.toString().padLeft(2, '0')}';
      }
      loading = false;
    });
  }

  Future<void> saveBool(String key, bool value) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setBool(key, value);
  }

  Future<void> _scheduleManualUpdate() async {
    int selectedMinutes = 5;

    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => StatefulBuilder(
        builder: (ctx, setDialogState) => AlertDialog(
          title: const Text('Ръчно обновяване'),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text('Планирай еднократно обновяване след:'),
              const SizedBox(height: 20),
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  IconButton.filledTonal(
                    icon: const Icon(Icons.remove),
                    onPressed: selectedMinutes > 1
                        ? () => setDialogState(() => selectedMinutes--)
                        : null,
                  ),
                  Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 20),
                    child: Column(
                      children: [
                        Text(
                          '$selectedMinutes',
                          style: Theme.of(ctx).textTheme.displaySmall?.copyWith(
                                fontWeight: FontWeight.bold,
                              ),
                        ),
                        Text(
                          selectedMinutes == 1 ? 'минута' : 'минути',
                          style: Theme.of(ctx).textTheme.bodySmall,
                        ),
                      ],
                    ),
                  ),
                  IconButton.filledTonal(
                    icon: const Icon(Icons.add),
                    onPressed: selectedMinutes < 60
                        ? () => setDialogState(() => selectedMinutes++)
                        : null,
                  ),
                ],
              ),
              const SizedBox(height: 8),
              Slider(
                value: selectedMinutes.toDouble(),
                min: 1,
                max: 60,
                divisions: 59,
                label: '$selectedMinutes мин',
                onChanged: (val) =>
                    setDialogState(() => selectedMinutes = val.round()),
              ),
            ],
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(ctx, false),
              child: const Text('Отказ'),
            ),
            FilledButton.icon(
              onPressed: () => Navigator.pop(ctx, true),
              icon: const Icon(Icons.schedule_rounded),
              label: const Text('Планирай'),
            ),
          ],
        ),
      ),
    );

    if (confirmed == true && mounted) {
      await GTFSTaskManager.scheduleOneOff(
        minutes: selectedMinutes,
        wifiOnly: wifiOnly,
      );

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            'Обновяването е планирано след $selectedMinutes '
            '${selectedMinutes == 1 ? 'минута' : 'минути'}.',
          ),
          behavior: SnackBarBehavior.floating,
        ),
      );
    }
  }

  Future<void> _confirmClearData() async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Изчистване на данни'),
        content: const Text(
          'Това ще изтрие локалната база данни с разписания. '
          'Ще трябва да ги изтеглите отново при следващото стартиране.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('Отказ'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(ctx, true),
            style: FilledButton.styleFrom(
              backgroundColor: Theme.of(context).colorScheme.error,
            ),
            child: const Text('Изчисти'),
          ),
        ],
      ),
    );

    if (confirmed == true && mounted) {
      if (mounted) {
        Navigator.of(context).pushAndRemoveUntil(
          MaterialPageRoute(builder: (_) => DatabaseLoadingScreen(force: true)),
          (route) => false,
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      backgroundColor: theme.colorScheme.surfaceContainerLowest,
      body: loading
          ? const Center(child: CircularProgressIndicator())
          : CustomScrollView(
              slivers: [
                SliverAppBar.large(
                  title: const Text('Настройки'),
                  centerTitle: false,
                  backgroundColor: theme.colorScheme.surfaceContainerLowest,
                ),
                SliverPadding(
                  padding: const EdgeInsets.fromLTRB(16, 0, 16, 32),
                  sliver: SliverList(
                    delegate: SliverChildListDelegate([
                      // ── Update card ──────────────────────────────────
                      _CardSection(
                        children: [
                          SwitchRow(
                            icon: Icons.sync_rounded,
                            iconColor: theme.colorScheme.primary,
                            title: 'Автоматично обновяване',
                            subtitle: 'Два пъти дневно',
                            value: autoUpdate,
                            onChanged: (val) async {
                              setState(() => autoUpdate = val);
                              await saveBool('settings_auto_update', val);
                              await GTFSTaskManager.reschedule(autoUpdate: val, wifiOnly: wifiOnly);
                            },
                          ),
                          _Divider(),
                          SwitchRow(
                            icon: Icons.wifi_rounded,
                            iconColor: theme.colorScheme.primary,
                            title: 'Само през Wi-Fi',
                            subtitle: '~40 MB на обновяване',
                            value: wifiOnly,
                            enabled: autoUpdate,
                            onChanged: autoUpdate
                                ? (val) async {
                                    setState(() => wifiOnly = val);
                                    await saveBool('settings_wifi_only', val);
                                    await GTFSTaskManager.reschedule(autoUpdate: autoUpdate, wifiOnly: val);
                                  }
                                : null,
                          ),
                          _Divider(),
                          SwitchRow(
                            icon: Icons.notifications_outlined,
                            iconColor: theme.colorScheme.primary,
                            title: 'Известия',
                            subtitle: 'След успешно обновяване',
                            value: updateNotifications,
                            enabled: autoUpdate,
                            onChanged: autoUpdate
                                ? (val) async {
                                    setState(() => updateNotifications = val);
                                    await saveBool(
                                        'settings_update_notifications', val);
                                  }
                                : null,
                          ),
                        ],
                      ),
                      const SizedBox(height: 12),

                      // ── Status + manual trigger ───────────────────────
                      _CardSection(
                        children: [
                          InfoRow(
                            icon: Icons.history_rounded,
                            iconColor: theme.colorScheme.secondary,
                            title: 'Последно обновяване',
                            value: lastUpdated,
                          ),
                          _Divider(),
                          TapRow(
                            icon: Icons.schedule_rounded,
                            iconColor: theme.colorScheme.tertiary,
                            title: 'Ръчно обновяване',
                            subtitle: 'Планирай еднократно',
                            onTap: _scheduleManualUpdate,
                          ),
                        ],
                      ),
                      const SizedBox(height: 24),

                      // ── Danger zone ──────────────────────────────────
                      _SectionLabel(label: 'Данни'),
                      const SizedBox(height: 8),
                      _CardSection(
                        children: [
                          TapRow(
                            icon: Icons.delete_outline_rounded,
                            iconColor: theme.colorScheme.error,
                            title: 'Изчисти локалните данни',
                            subtitle: 'Изтрива базата данни с разписания',
                            onTap: _confirmClearData,
                            destructive: true,
                          ),
                        ],
                      ),
                      const SizedBox(height: 24),

                      // ── About ────────────────────────────────────────
                      _SectionLabel(label: 'За приложението'),
                      const SizedBox(height: 8),
                      _CardSection(
                        children: [
                          InfoRow(
                            icon: Icons.info_outline_rounded,
                            iconColor: theme.colorScheme.secondary,
                            title: 'Версия',
                            value: '1.0.0',
                          ),
                          _Divider(),
                          InfoRow(
                            icon: Icons.directions_bus_rounded,
                            iconColor: theme.colorScheme.secondary,
                            title: 'Данни',
                            value: 'GTFS — Център за градска мобилност',
                          ),
                        ],
                      ),
                    ]),
                  ),
                ),
              ],
            ),
    );
  }
}

// ── Reusable card wrapper ───────────────────────────────────────────────────

class _CardSection extends StatelessWidget {
  final List<Widget> children;
  const _CardSection({required this.children});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Container(
      decoration: BoxDecoration(
        color: theme.colorScheme.surfaceContainerLow,
        borderRadius: BorderRadius.circular(20),
      ),
      clipBehavior: Clip.antiAlias,
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: children,
      ),
    );
  }
}

class _Divider extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return Divider(
      height: 1,
      indent: 64,
      endIndent: 0,
      color: Theme.of(context).colorScheme.outlineVariant.withOpacity(0.4),
    );
  }
}

class _SectionLabel extends StatelessWidget {
  final String label;
  const _SectionLabel({required this.label});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Padding(
      padding: const EdgeInsets.only(left: 4),
      child: Text(
        label,
        style: theme.textTheme.labelMedium?.copyWith(
          color: theme.colorScheme.onSurfaceVariant,
          fontWeight: FontWeight.w600,
          letterSpacing: 0.5,
        ),
      ),
    );
  }
}

// ── Row variants ────────────────────────────────────────────────────────────

class _RowIcon extends StatelessWidget {
  final IconData icon;
  final Color color;
  final bool enabled;

  const _RowIcon({
    required this.icon,
    required this.color,
    this.enabled = true,
  });

  @override
  Widget build(BuildContext context) {
    final c = enabled ? color : Theme.of(context).colorScheme.outlineVariant;
    return Container(
      width: 36,
      height: 36,
      decoration: BoxDecoration(
        color: c.withOpacity(0.12),
        borderRadius: BorderRadius.circular(10),
      ),
      child: Icon(icon, color: c, size: 20),
    );
  }
}

class SwitchRow extends StatelessWidget {
  final IconData icon;
  final Color iconColor;
  final String title;
  final String subtitle;
  final bool value;
  final bool enabled;
  final ValueChanged<bool>? onChanged;

  const SwitchRow({
    required this.icon,
    required this.iconColor,
    required this.title,
    required this.subtitle,
    required this.value,
    this.enabled = true,
    this.onChanged,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final fade = enabled ? 1.0 : 0.4;

    return Opacity(
      opacity: fade,
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
        child: Row(
          children: [
            _RowIcon(icon: icon, color: iconColor, enabled: enabled),
            const SizedBox(width: 14),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(title,
                      style: theme.textTheme.bodyMedium
                          ?.copyWith(fontWeight: FontWeight.w600)),
                  const SizedBox(height: 2),
                  Text(subtitle,
                      style: theme.textTheme.bodySmall?.copyWith(
                          color: theme.colorScheme.onSurfaceVariant)),
                ],
              ),
            ),
            Switch(value: value, onChanged: onChanged),
          ],
        ),
      ),
    );
  }
}

class TapRow extends StatelessWidget {
  final IconData icon;
  final Color iconColor;
  final String title;
  final String subtitle;
  final VoidCallback? onTap;
  final bool destructive;

  const TapRow({
    required this.icon,
    required this.iconColor,
    required this.title,
    required this.subtitle,
    this.onTap,
    this.destructive = false,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final titleColor =
        destructive ? theme.colorScheme.error : theme.colorScheme.onSurface;

    return InkWell(
      onTap: onTap,
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
        child: Row(
          children: [
            _RowIcon(icon: icon, color: iconColor),
            const SizedBox(width: 14),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(title,
                      style: theme.textTheme.bodyMedium?.copyWith(
                          fontWeight: FontWeight.w600, color: titleColor)),
                  const SizedBox(height: 2),
                  Text(subtitle,
                      style: theme.textTheme.bodySmall?.copyWith(
                          color: theme.colorScheme.onSurfaceVariant)),
                ],
              ),
            ),
            Icon(Icons.chevron_right_rounded,
                size: 20, color: theme.colorScheme.outline),
          ],
        ),
      ),
    );
  }
}

class InfoRow extends StatelessWidget {
  final IconData icon;
  final Color iconColor;
  final String title;
  final String value;

  const InfoRow({
    required this.icon,
    required this.iconColor,
    required this.title,
    required this.value,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      child: Row(
        children: [
          _RowIcon(icon: icon, color: iconColor),
          const SizedBox(width: 14),
          Expanded(
            child: Text(title,
                style: theme.textTheme.bodyMedium
                    ?.copyWith(fontWeight: FontWeight.w600)),
          ),
          Text(value,
              style: theme.textTheme.bodySmall?.copyWith(
                  color: theme.colorScheme.onSurfaceVariant)),
        ],
      ),
    );
  }
}