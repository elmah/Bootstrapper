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

To view error log pages, simply naviagte to any URL within your web application
that contains the word `elmah`, `errors` or `errorlog`. So if your
web application is installed at `http://www.example.com/` then any of the
following URLs can be used to reach the ELMAH web interface:

- `http://www.example.com/elmah`
- `http://www.example.com/errors`
- `http://www.example.com/errorlog`
- `http://www.example.com/admin/errorlog`
- `http://www.example.com/foo/bar/errorlog`

ELMAH Bootstrapper can also select the `ErrorLog` implementation based on
certain conventions. For example, if you add the folders `App_Data\errors\xmlstore`
under your web application root then errors will automatically be logged there
as XML files, using [`XmlFileErrorLog`][xmllog].

ELMAH ships with six `ErrorLog` implementations that use a database as a
persistent store:

- `SqlErrorLog`
- `SQLiteErrorLog`
- `SqlServerCompactErrorLog`
- `OracleErrorLog`
- `MySqlErrorLog`
- `PgsqlErrorLog`

To use one of these, create a [connection string entry][csadd] in your
`web.config` named `elmah:LOGNAME` where `LOGNAME` is the error log name
minus the `ErrorLog` suffix (case-insensitive). So to use `SqlErrorLog`,
create a connection string named `elmah:sql`.

If the `ErrorLog` implementation requires additional settings, these can
be supplied via [`appSettings`][appSettings] using the naming convention
`elmah:LOGNAME:KEY`, e.g. `elmah:sql:applicationName`.

With ELMAH Bootstrapper, you can also [filter errors][filter] from being
logged or e-mailed by simply adding a file named `Elmah.ErrorFilter.config` in
your web application root. The content of the file should be the desired
filtering rules in XML, like:

    <and>
      <greater binding="HttpStatusCode" value="399" type="Int32" />
      <lesser  binding="HttpStatusCode" value="500" type="Int32" />
    </and>

If the file doesn't start with the angle bracket `<` then you can throw in
an error filtering rule expressed entirely in [JScript][jsfltr]:

    // @assembly mscorlib
    // @assembly System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
    // @import System.IO
    // @import System.Web

    $.HttpStatusCode == 404
    || $.BaseException instanceof FileNotFoundException
    || $.BaseException instanceof HttpRequestValidationException
    /* Using RegExp below (see http://msdn.microsoft.com/en-us/library/h6e2eb7w.aspx) */
    || $.Context.Request.UserAgent.match(/crawler/i)
    || $.Context.Request.ServerVariables['REMOTE_ADDR'] == '127.0.0.1' // IPv4 only

For some inspiration on rules, see [Error Filtering Examples][fltreg].

ELMAH Bootstrapper can also refresh the error filtering rules whenever the
`Elmah.ErrorFilter.config` file is updated without causing the application to
restart!

ELMAH Bootstrapper also allows control over who is authorized to see the
built-in error log web. The NuGet package automatically adds a file called
`Elmah.Athz.config` with a single rule permitting only local requests:

    @local

Each line of the file represents an access grant or denial and here is what to
keep in mind:

- To allow users access, simply list their accounts on separate lines.
- An asterisk (`*`) represents all authenticated users.
- An question mark (?) represents all anonymous users.
- Roles are identified with a caret prefix (`^`), like this: `^admins`
- A line starting with an exclamation mark or bang (`!`) is a denial;
  otherwise a grant.
- Denials take precedence over grants.
- A hash or pound (`#`) delimits a single-line comment.
- `@local` is special and represents a local request.

Here is an example `Elmah.Athz.config`:

    # this is a single-line comment

    !?      # deny anonymous users
    ^admins # allow users in admin role
    ^ops    # allow users in ops role
    !bob    # deny Bob
    alice   # allow Alice
    @local  # allow local requests

Changes to authorization file are effective immediately and without requiring
a restart of the web application.


  [elmah]: https://elmah.github.io/
  [pkg]: https://www.nuget.org/packages/elmah.bootstrapper
  [aspnet]: http://www.asp.net/
  [regmod]: https://msdn.microsoft.com/en-us/library/system.web.httpapplication.registermodule.aspx
  [appstart]: https://msdn.microsoft.com/en-us/library/system.web.preapplicationstartmethodattribute.aspx
  [xmllog]: https://www.nuget.org/packages/elmah.xml/
  [csadd]: https://msdn.microsoft.com/en-us/library/vstudio/htw9h4z3(v=vs.100).aspx
  [appSettings]: https://msdn.microsoft.com/en-us/library/vstudio/ms228154(v=vs.100).aspx
  [filter]: https://elmah.github.io/a/error-filtering/
  [fltreg]: https://elmah.github.io/a/error-filtering/
  [jsfltr]: https://elmah.github.io/a/error-filtering/#using-jscript
