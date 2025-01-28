using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TramlineFive.Common.Messages;

public record VirtualTablesViewRenderChangedMessage(bool isRendered);
