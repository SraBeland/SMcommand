# SMcommand
SMcommand is a command line (Windows) application that demonstrates the use of the System Matrix API and provides a method of controlling some functions of the Prismview displays controlled by System Matrix.  

*Written by Graham Beland*

**-a, --address**         System Matrix IP address [example: localhost, 192.168.1.1]  
**--port**                The port System Matrix is running on [int] default is 82  
**-u, --username**        System Matrix API username  
**-p, --password**        System Matrix API password  
**-s, --save**            Save the settings (username, password, etc)  
**--starttestpattern**    Starts a test pattern [Pattern] [Brightness] Patterns:[SolidRed, SolidGreen, SolidBlue, SolidWhite, CycleColors, LinesVertical, LinesDiagonal, GridColors, GridNumbered]  
**--stoptestpattern**     Stops test pattern(s)  
**-l, --listdisplays**    Prints all messages to standard output.  
**-j, --savejson**        Saves the Monitoring data to Monitoring.json  
**-b, --brightness**      The brightness value to set to [int] (-1 to revert to default value)   
**-z, --nofetch**         Prevents the Monitoring data from being retrieved - if other commands require the data it will still be retrieved.  
**-d, --displays**        Display ID (GUID(s) or Name(s)) to control (do not include to send to all display). Can be one or more (separated by a space)
**--refreshheader**       Send a header on all controllers.  
**--poweron**             Power power supplies On.  
**--poweroff**            Power power supplies Off.  
**--powercycle**          Cycle Power power supplies.  
**--enableoutput**        Enables output on all controllers.  
**--disableoutput**       Disables output on all controllers.  
**--help**                Display this help screen.  
**--version**             Display the version.  