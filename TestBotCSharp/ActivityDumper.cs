using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Microsoft.Bot.Connector;


namespace TestBotCSharp
{
    public class ActivityDumper
    {
        public static string ActivityDump(Activity act)
        {
            if (null != act) 
            {
                string str = JsonConvert.SerializeObject(act,Formatting.Indented);
                //Replace /r/n with /Br
                str = str.Replace("<", "«");
                str = str.Replace(">", "»");
                str = str.Replace("\r\n", "<br />");
                
                return str;
            }
            else
            {
                return null;
            }
        }
    }
}
