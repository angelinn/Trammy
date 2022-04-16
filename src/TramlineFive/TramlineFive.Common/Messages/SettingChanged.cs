using System;
using System.Collections.Generic;
using System.Text;

namespace TramlineFive.Common.Messages;

public record SettingChanged<T>(string Name, T Value);
