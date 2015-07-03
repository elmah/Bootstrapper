# ELMAH Bootstrapper

ELMAH Bootstrapper automatically registers [ELMAH][elmah] during application
start-up. It is [available as a NuGet package][pkg] and designed to work with 
ASP.NET 4.x web applications only.

[ELMAH (Error Logging Modules and Handlers)][elmah] is an ASP.NET web
application-wide error logging facility that is completely pluggable. It can be
enabled without changing a single line of your application code so there is no
need for re-compilation or re-deployment.

ELMAH is wired into an application by registering its modules and handlers via
the application's configuration (`web.config`). Getting all the registration
configuration right can occasionally get frustrating as the details can vary
between hosting servers, like IIS, and when going from development to
production.

Recent versions of ASP.NET have made it possible to [dynamically register
modules at run-time][regmod], during [application start-up][appstart], and 
that is exactly what ELMAH Bootstrapper does.

Once you add ELMAH Bootstrapper via NuGet, it will automatically add ELMAH (if
not already installed) and register it with your web application without any
need for configuration whatsoever.

To view error log pages, you simply naviagte to any URL within your web
application that contains the word `elmah`, `errors` or `errorlog`. So if your
web application is installed at `http://www.example.com/` then any of the
following URLs can be used to reach the ELMAH web interface:

- `http://www.example.com/elmah`
- `http://www.example.com/errors`
- `http://www.example.com/errorlog`
- `http://www.example.com/admin/errorlog`
- `http://www.example.com/foo/bar/errorlog`

Finally, if you add the folders `App_Data\errors\xmlstore` under your web
application root then ELMAH Bootstrap will also have ELMAH automatically log
errors there as XML files (using [`XmlFileErrorLog`][xmllog]).


  [elmah]: https://elmah.github.io/
  [pkg]: https://www.nuget.org/packages/elmah.bootstrapper
  [aspnet]: http://www.asp.net/
  [regmod]: https://msdn.microsoft.com/en-us/library/system.web.httpapplication.registermodule.aspx
  [appstart]: https://msdn.microsoft.com/en-us/library/system.web.preapplicationstartmethodattribute.aspx
  [xmllog]: https://www.nuget.org/packages/elmah.xml/