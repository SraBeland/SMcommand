# SMcommand
Command line utility to control System Matrix Prismview displays.

-a, --address         System Matrix IP address [example: localhost, 192.168.1.1]  
--port                The port System Matrix is running on [int] default is 82  
--starttestpattern    Starts a test pattern [SolidRed, SolidGreen, SolidBlue, SolidWhite, CycleColors, LinesVertical, LinesDiagonal, GridColors, GridNumbered]  
--stoptestpattern     Stop test pattern  
-u, --username        System Matrix API username  
-p, --password        System Matrix API password  
-s, --save            Save the settings (username, password etc)  
-l, --listdisplays    Prints all messages to standard output.  
--refreshheader       Send a header on all controllers.  
--poweron             Power power supplies On.  
--poweroff            Power power supplies Off.  
--powercycle          Cycle Power power supplies.  
--enableoutput        Enables output on all controllers.  
--disableoutput       Disables output on all controllers.  
-j, --savejson        Saves the Monitoring data to Monitoring.json  
-b, --brightness      Sets all displays to a brightness [int] (-1 to revert to default value)   
-z, --nofetch         Prevents the Monitoring data from being retrieved - if other commands require the data it will still be retrieved.  
-d, --displays        Display ID (GUID or Name) to control (do not include to send to all display)  
--help                Display this help screen.  