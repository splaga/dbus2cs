using System;
using System.Collections.Generic;
using System.Text;

namespace dbus2cs
{
    public class TabStringBuilder
    {
        public StringBuilder StringBuilder;
        public TabStringBuilder()
        {
            StringBuilder = new StringBuilder();
        }

        public TabStringBuilder Append(object o)
        {
            StringBuilder.Append(o);
            return this;
        }

        public TabStringBuilder Append(string value)
        {
            StringBuilder.Append(value);
            return this;
        }

        public TabStringBuilder Append(string format, params object[] args)
        {
            return Append(0, format, args);
        }

        public TabStringBuilder Append(int tabCount, string format, params object[] args)
        {
            WriteTabs(tabCount);
            StringBuilder.AppendFormat(format, args);
            return this;
        }

        public TabStringBuilder AppendLine()
        {
            StringBuilder.AppendLine();
            return this;
        }

        public TabStringBuilder AppendLine(string format, params object[] args)
        {
            return AppendLine(0, format, args);
        }

        public TabStringBuilder AppendLine(int tabCount, string format, params object[] args)
        {
            WriteTabs(tabCount);
            StringBuilder.AppendLine(string.Format(format, args));
            return this;
        }


        public void WriteTabs(int tabcount)
        {
            for (int i = 0; i < tabcount; i++)
                StringBuilder.Append("\t");
        }

        public void Replace(string find, string replace)
        {
            StringBuilder.Replace(find, replace);
        }

        public override string ToString()
        {
            return StringBuilder.ToString();
        }
    }
}
