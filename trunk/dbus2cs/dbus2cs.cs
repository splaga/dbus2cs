using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace dbus2cs
{
    class dbus2cs
    {
        static string fileOrPath, outputFile = null;
        static Dictionary<string, string> TypeMap = new Dictionary<string,string>();
        static StringBuilder StructsEtc = new StringBuilder();
        static Dictionary<string, string> Replaces = new Dictionary<string, string>();
        static Dictionary<string, string> StructReplaces = new Dictionary<string, string>();
        static Dictionary<string, string> VariableReplaces = new Dictionary<string, string>();
        static string Namespace = string.Empty;
        static string License = string.Empty;
        static int structIndex = 1;
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(PrintHelp());
                return;
            }
            string ret = ParseArgs(args);
            if (ret != string.Empty)
            {
                Console.WriteLine(ret);
                return;
            }
            PopulateTypeMap(TypeMap);
            if (File.Exists(fileOrPath))
            {
                Dbus2Cs(fileOrPath);
            }
            else if (Directory.Exists(Path.GetDirectoryName(fileOrPath)))
            {
                string dir = Path.GetDirectoryName(fileOrPath);
                string fileOrFilter = Path.GetFileName(fileOrPath);
                foreach (string file in Directory.GetFiles(dir, fileOrFilter))
                {
                    Dbus2Cs(file);
                }
            }
            else if (fileOrPath.ToLower().StartsWith("http"))
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(fileOrPath);
                Dbus2Cs(req.GetResponse().GetResponseStream(), fileOrPath);
            }
            else if (fileOrPath == "-")
            {
                if (outputFile == null)
                    outputFile = "-";
                Dbus2Cs(Console.OpenStandardInput(), "-");
            }
        }

        public static void Dbus2Cs(string filename)
        {
            Dbus2Cs(File.OpenRead(filename), filename);
        }

        public static void Dbus2Cs(Stream stream, string filename)
        {
            if (!(outputFile == "-"))
                Console.Write("Processing {0}...", filename);
            XmlDocument doc = new XmlDocument();
            doc.Load(stream);
            TabStringBuilder builder = new TabStringBuilder();
            string csFile = outputFile == null ? Path.GetFileName(filename) + ".cs" : outputFile;
            WriteFileHeader(string.Empty, builder);
            WriteUsings(doc, builder);
            WriteNamespace(doc.SelectSingleNode("node"), builder);
            if (Replaces.Count > 0)
            {
                foreach (KeyValuePair<string, string> replace in Replaces)
                    builder.Replace(replace.Key, replace.Value);
            }
            Stream outStream = null;
            if (csFile == "-")
                outStream = Console.OpenStandardOutput();
            else if (csFile != null)
                outStream = File.Open(csFile, FileMode.Create);
            using (StreamWriter writer = new StreamWriter(outStream))
                writer.Write(builder.ToString());
            if (!(outputFile == "-"))
                Console.WriteLine("Done");
        }

        public static void WriteInterface(XmlNode node, TabStringBuilder builder)
        {
            foreach(XmlNode doc in node.ChildNodes)
                if (doc.Name == "doc:doc")
                {
                    builder.AppendLine(1, "/// <summary>");
                    foreach (string line in doc.InnerText.Trim().Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.None))
                        builder.AppendLine(1, "/// {0}", line.Trim());
                    builder.AppendLine(1, "/// </summary>");
                }
            string raw = node.Attributes["name"].Value;
            builder.AppendLine(1, "[Interface(\"{0}\")]", raw);
            string[] split = raw.Split('.');
            builder.Append(1, "public interface {0}{1}\t{{{1}", split[split.Length - 1], Environment.NewLine);
            foreach (XmlNode typeNode in node.ChildNodes)
            {
                switch (typeNode.Name)
                {
                    case ("method"):
                        //if (typeNode.Attributes["name"].Value.ToLower().StartsWith("get") || typeNode.Attributes["name"].Value.ToLower().StartsWith("set"))
                        //{
                        //    WriteProperty(typeNode, builder);
                        //}
                        //else
                        WriteComment(typeNode, builder);
                            WriteMethod(typeNode, builder);
                        break;
                    case ("signal"):
                        WriteComment(typeNode, builder);
                        WriteEvent(typeNode, builder);
                        break;
                    case ("error"):
                        break;//make custom error types?
                }
            }
            builder.AppendLine(1, "}}{0}", Environment.NewLine);
        }

        public static void WriteComment(XmlNode node, TabStringBuilder builder)
        {
            List<string> description = new List<string>(), remark = new List<string>(), returns = new List<string>();
            List<KeyValuePair<string, string[]>> args = new List<KeyValuePair<string, string[]>>();//keep order
            foreach (XmlNode docOrArg in node.ChildNodes)
                switch (docOrArg.Name.ToLower())
                {
                    case ("doc:doc"):
                        foreach (XmlNode descOrRemark in docOrArg.ChildNodes)
                        {
                            if (descOrRemark.Name.ToLower() == "doc:description")
                                description.AddRange(descOrRemark.InnerText.Trim().Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.None));
                            if (descOrRemark.Name.ToLower() == "doc:inote")
                                remark.AddRange(descOrRemark.InnerText.Trim().Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.None));
                        }
                        break;
                    case ("arg"):
                        if (docOrArg.Attributes["direction"] != null && docOrArg.Attributes["name"] != null)
                            if (docOrArg.Attributes["direction"].Value == "in")
                            {
                                foreach (XmlNode docNode in docOrArg.ChildNodes)
                                    foreach (XmlNode summary in docNode.ChildNodes)
                                    {
                                        if (docNode.Name.ToLower() == "doc:doc" && summary.Name.ToLower() == "doc:summary")
                                        {
                                            args.Add(new KeyValuePair<string, string[]>(docOrArg.Attributes["name"].Value, summary.InnerText.Trim().Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.None)));
                                        }
                                    }
                            }
                            else if (docOrArg.Attributes["direction"].Value == "out")
                            {
                                foreach (XmlNode docNode in docOrArg.ChildNodes)
                                    foreach (XmlNode summary in docNode.ChildNodes)
                                    {
                                        if (docNode.Name.ToLower() == "doc:doc" && summary.Name.ToLower() == "doc:summary")
                                            returns.AddRange(summary.InnerText.Trim().Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.None));
                                    }
                            }
                        break;
                }
            if (description.Count > 0 && description[0].Trim().Length > 0)
            {
                builder.AppendLine(2, "/// <summary>");
                foreach (string line in description)
                    builder.AppendLine(2, "/// {0}", line.Trim());
                builder.AppendLine(2, "/// </summary>");
            }
            if (remark.Count > 0 && remark[0].Trim().Length > 0)
            {
                builder.AppendLine(2, "/// <remarks>");
                foreach (string line in remark)
                    builder.AppendLine(2, "/// {0}", line.Trim());
                builder.AppendLine(2, "/// </remarks>");
            }
            if (args.Count > 0)
            {
                if (args.Count > 0)
                    foreach (KeyValuePair<string, string[]> pair in args)
                    {
                        builder.AppendLine(2, "/// <param name=\"{0}\">", pair.Key);
                        foreach (string line in pair.Value)
                            builder.AppendLine(2, "/// {0}", line);
                        builder.AppendLine(2, "/// </param>");
                    }
            }
            if (returns.Count > 0 && returns[0].Trim().Length > 0)
            {
                builder.AppendLine(2, "/// <returns>");
                foreach (string ret in returns)
                    builder.AppendLine(2, "/// {0}", ret.Trim());
                builder.AppendLine(2, "/// </returns>");
            }
        }

        public static void WriteProperty(XmlNode node, TabStringBuilder builder)
        {
            builder.AppendLine(2, "{0} {1} {{ {2} }}", GetReturnType(node), node.Attributes["name"].Value, GetParameters(node));
        }

        public static void WriteMethod(XmlNode node, TabStringBuilder builder)
        {
            builder.AppendLine(2, "{0} {1}({2});", GetReturnType(node), node.Attributes["name"].Value, GetParameters(node));
        }

        public static string GetParameters(XmlNode methodNode)
        {
            StringBuilder builder = new StringBuilder();
            XmlNodeList list = methodNode.SelectNodes("arg[not(contains(@direction, 'out'))]");
            for (int i = 0; i < list.Count; i++)
            {
                builder.AppendFormat("{0} {1}", ConvertTypes(list[i].Attributes["type"].Value), list[i].Attributes["name"].Value);
                if (i < list.Count - 1)
                    builder.Append(", ");
            }
            return builder.ToString();
        }

        /// <summary>
        /// Convert types from the Xml introspection format, to C#, including the creation of new structs
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ConvertTypes(string value, ref int index)
        {
            if (value[index] == 'a')//determine if array of basic, or dictionary entries
            {
                index++;
                if (value[index] == '(')
                {
                    return ConvertTypes(value, ref index);//string.Format("{0}[]", CreateStruct(value, ref index, ref structIndex));
                }
                else if (value[index] == '{')
                {
                    return ConvertTypes(value, ref index);
                }
                return string.Format("{0}[]", ConvertTypes(value, ref index));
            }
            if (value[index] == '{')
            {
                if (index > 0 && value[index - 1] != 'a')
                    Console.WriteLine("Warning: dictionary entries found, but no array ('a') was specified before it.");
                index++;
                string key = TypeMap[value[index].ToString()];
                index++;
                return string.Format("Dictionary<{0}, {1}>", key, ConvertTypes(value, ref index));
            }
            if (value[index] == '(')
            {
                index++;
                return string.Format("{0}[]", CreateStruct(value, ref index));
            }
            return TypeMap[value[index].ToString()];
        }

        public static string ConvertTypes(string value)
        {
            int index = 0;
            return ConvertTypes(value, ref index);
        }
        public static string CreateStruct(string input, ref int index)
        {
            int varIndex = 1;
            string thisStruct = "Struct" + structIndex++;
            if (StructReplaces.ContainsKey(thisStruct))
                thisStruct = StructReplaces[thisStruct];
            StructsEtc.AppendFormat("{0}public struct {1}{2}\t{{{2}", "\t", thisStruct, Environment.NewLine);
            while (index < input.Length && input[index] != ')' && input[index] != '}')
            {
                string varName = "Var" + varIndex++;
                if (VariableReplaces.ContainsKey(thisStruct + "/" + varName))
                    varName = VariableReplaces[thisStruct + "/" + varName];
                StructsEtc.AppendFormat("{0}public {1} {2};{3}", "\t\t", ConvertTypes(input, ref index), varName, Environment.NewLine);
                index++;
            }
            StructsEtc.AppendFormat("\t}}{0}{0}", Environment.NewLine);
            return thisStruct;
        }

        public static string GetReturnType(XmlNode methodNode)
        {
            XmlNodeList list = methodNode.SelectNodes("arg[contains(@direction, 'out')]");
            if (list.Count > 0)
                return ConvertTypes(list[0].Attributes["type"].Value);
            return "void";
        }

        public static void WriteEvent(XmlNode node, TabStringBuilder builder)
        {
            builder.AppendLine(2, "event {0} {1};", CreateDelegate(node, builder), node.Attributes["name"].Value);
        }

        public static string CreateDelegate(XmlNode node, TabStringBuilder builder)
        {
            StructsEtc.AppendFormat("\tpublic delegate {0} {1}({2});{3}{3}", GetReturnType(node), node.Attributes["name"].Value + "Handler", GetParameters(node), Environment.NewLine);
            return node.Attributes["name"].Value + "Handler";
        }

        public static void WriteNamespace(XmlNode node, TabStringBuilder builder)
        {
            //string[] split = node.FirstChild.Attributes["name"].Value.Split('.');
            //builder.Append("namespace ");
            //for (int i = 1; i < split.Length; i++)
            //{
            //    builder.Append(split[i]);
            //    if (i < split.Length - 1)
            //        builder.Append(".");
            //}
            builder.Append("namespace {0}", Namespace == string.Empty ? "DefaultNamespace" : Namespace);
            builder.AppendLine("{0}{{", Environment.NewLine);
            foreach (XmlNode inter in node.SelectNodes("interface"))
            {
                WriteInterface(inter, builder);
            }
            builder.Append(StructsEtc);
            builder.Append("}");
        }

        public static void WriteUsings(XmlDocument doc, TabStringBuilder builder)
        {
            builder.AppendLine("using System;").AppendLine("using System.Collections.Generic;").AppendLine("using NDesk.DBus;").AppendLine();
        }

        public static void WriteFileHeader(string filename, TabStringBuilder builder)
        {
            builder.AppendLine("/*This document was generated with a tool.{0}dbus2cs version {1}*/", Environment.NewLine, System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion);
            builder.AppendLine(License);
        }

        public static string PrintHelp()
        {
            return string.Format("usage: {0} {1} [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", "dbus2cs", "filename|httpURL", "-n namespace", "-o output file", "-l licensefile", "-r search:replace", "-v <struct>[/<variable>]:<new struct|variable>", "-?|help");
            //-o compile to assembly using environ arg (G)MCS[lookup which one is appropriate]
            //http://git.freesmartphone.org/?p=specs.git;a=blob_plain;f=otapi/org.freesmartphone.GSM.SIM.xml.in;hb=HEAD
        }

        public static string PrintLongHelp()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Options:");
            builder.AppendLine("\tFilename of DBus introspection Xml to parse. stdin is supported via \"-\"");
            builder.AppendLine("\t-n:");
            builder.AppendLine("\t\tSet namespace to use");
            builder.AppendLine("\t-o:");
            builder.AppendLine("\t\tSet output filename. stdout is supported via \"-\"");
            builder.AppendLine("\t-l:");
            builder.AppendLine("\t\tRead in license file to include at top of each code file");
            
            builder.AppendLine("\t-r:");
            builder.AppendLine("\t\tSearch and replace for arbitrary strings in the finished code file");
            builder.AppendLine("\t-v:");
            builder.AppendLine("\t\tRename the struct or variable name");
            //builder.AppendLine("\t\tTo set the namespace, the syntax is oldNamespace:newNamespace");
            builder.AppendLine("\t\tTo set the class, the syntax is oldStruct:newStruct");
            builder.AppendLine("\t\tTo set the variable, the syntax is newStruct/oldVariable:newVariable");
            builder.AppendLine("\t-?|-help");
            builder.AppendLine("\t\tThis text");
            return builder.ToString();
        }
        //..\..\testdocs\otapi\org.freesmartphone.GSM.SIM.xml.in -n org.freesmartphone.GSM.SIM -v Struct1:Homezone -v Homezone/Var1:Name -v Homezone/Var2:X -v Homezone/Var3:Y -v Homezone/Var4:Radius -v Struct2:Phonebook -v Phonebook/Var1:Index -v Phonebook/Var2:Name -v Phonebook/Var3:Number -v Struct3:Messagebook -v Messagebook/Var1:Index -v Messagebook/Var2:Status -v Messagebook/Var3:Number -v Messagebook/Var4:Content
        public static string ParseArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case ("-n"):
                        if (args[i + 1].StartsWith("-"))
                            return "Namespace after -n expected";
                        Namespace = args[i + 1];
                        i++;
                        break;
                    case ("-l"):
                        if (args[i + 1].StartsWith("-"))
                            return "License file after -l expected";
                        License = File.ReadAllText(args[i + 1]);
                        i++;
                        break;
                    case ("-r"):
                        if (args[i + 1].StartsWith("-"))
                            return "Parameters expected after -r";
                        string[] replaces = args[i + 1].Split(':');
                        Replaces.Add(replaces[0], replaces[1]);
                        break;
                    case ("-o"):
                        if (args[i + 1].StartsWith("-") && args[i + 1] != "-")
                            return "Output filename expected after -o";
                        outputFile = args[i + 1];
                        i++;
                        break;
                    case ("-v"):
                        string[] splits = args[i + 1].Split(new string[] { "/" }, StringSplitOptions.None);
                        switch (splits.Length)
                        {
                            case (1)://struct
                                {
                                    string[] replace = splits[0].Split(':');
                                    StructReplaces.Add(replace[0], replace[1]);
                                }
                                break;
                            case (2)://variable
                                {
                                    string[] replace = splits[1].Split(':');
                                    VariableReplaces.Add(splits[0] + "/" + replace[0], replace[1]);
                                }
                                break;
                        }
                        i++;
                        break;
                    case ("-?"):
                    case ("-help"):
                    case ("--help"):
                    case ("/help"):
                    case ("/?"):
                        return PrintHelp() + Environment.NewLine + PrintLongHelp();
                    default:
                        if (fileOrPath != null)
                            return string.Format("Output file already specified: {0}", fileOrPath);
                        fileOrPath = args[i];
                        break;
                }
            }
            return string.Empty;
        }

        public static void PopulateTypeMap(Dictionary<string, string> map)
        {
            map.Add("y", "byte");
            map.Add("b", "bool");
            map.Add("n", "short");
            map.Add("q", "ushort");
            map.Add("i", "int");
            map.Add("u", "uint");
            map.Add("x", "long");
            map.Add("t", "ulong");
            map.Add("d", "double");
            map.Add("s", "string");
            map.Add("o", "string");
            map.Add("g", "[A type signature]");
            map.Add("a", "[Array or Dictionary]");
            //map.Add("r", "");//special
            map.Add("v", "object");
            //map.Add("e", "");
        }
    }
}
