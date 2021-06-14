using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rrs.Microsoft.Logging
{
    public class Log
    {
        public string LogName { get; set; }
        public EventId EventId { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public LogLevel LogLevel { get; set; }
        public List<object> Scope { get; set; }
    }
}
