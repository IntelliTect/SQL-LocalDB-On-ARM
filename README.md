# SQL-LocalDB-On-ARM
A package that enables SQL Server LocalDB on ARM-based Windows systems without using containers.

## Usage
This can be inserted into your startup. If you are not using an ARM processor or not using LocalDB in your connection string this will simply return your connection string. This way some of the people on the team can use an ARM system without impacting others.

```
using IntelliTect.LocalDbOnArm;
...
// Your connection string typically obtained from settings.
string connectionString = "MyConnectionString" 
connectionString = LocalDbOnArm.UpdateConnectionString(connectionString);
```
## How it works
1. Detect if the connection string is using localdb
2. Detect if the OS is ARM
3. Start LocalDB if it isn't running
4. Get the named pipe for LocalDB
5. Update the connection string with the named pipe.
