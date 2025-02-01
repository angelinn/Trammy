﻿using SkgtService.Models;
using SkgtService.Models.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TramlineFive.Common.Messages;

public record ShowRouteMessage(string Line, Direction Direction, TransportType TransportType);
