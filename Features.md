# Features #
  * Support for methods and events.
  * Support for nested structs.
  * Convert from a local Xml file, or pull from an http address.

# Planned features #
  * Convert by retrieving the introspection Xml from a DBus service.

# Usage #
dbus2cs.exe filename|httpURL [-n namespace] [-o output file] [-l licensefile] [-r search:replace] [-v `<`struct>[/`<`variable>]:<new struct|variable>] [-?|help]
Options:
> Filename of DBus introspection Xml to parse. stdin is supported via "-"

> -n:
> > Set namespace to use

> -o:
> > Set output filename. stdout is supported via "-"

> -l:
> > Read in license file to include at top of each code file

> -r:
> > Search and replace for arbitrary strings in the finished code file

> -v:
> > Rename the struct or variable name
> > To set the class, the syntax is oldStruct:newStruct
> > To set the variable, the syntax is newStruct/oldVariable:newVariable

> -?|-help
> > This text
# Short example #
dbus2cs.exe ..\..\testdocs\otapi\org.freesmartphone.GSM.SIM.xml.in
# Long example #
dbus2cs.exe ..\..\testdocs\otapi\org.freesmartphone.GSM.SIM.xml.in -n org.freesmartphone.GSM.SIM -v Struct1:Homezone -v Homezone/Var1:Name -v Homezone/Var2:X -v Homezone/Var3:Y -v Homezone/Var4:Radius -v Struct2:Phonebook -v Phonebook/Var1:Index -v Phonebook/Var2:Name -v Phonebook/Var3:Number -v Struct3:Messagebook -v Messagebook/Var1:Index -v Messagebook/Var2:Status -v Messagebook/Var3:Number -v Messagebook/Var4:Content