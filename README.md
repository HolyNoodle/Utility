# Information
For any kind of information or return on my porjects feel free to send an email to : kbriffaut@gmail.com

# Utility
Provide a set of tools to work with. Localisation though Json object file, SQL Server Dal through reflection/attribute using stored procedure (Close to Entity but more database flexible).

# Dal Documentation
This Dal can be found in the namespace : HolyNoodle.Utility.Dal.

It allows you to create models and bind them to a stored procedure.

## Configuration

You can use configuration file to init the connection string.
```xml
  <connectionStrings>
    <add name="myProdDb" connectionString="your connection string" />
  </connectionStrings>
  <appSettings>
    <add key="holynoodle:DefaultConnectionString" value="myProdDb" />
  </appSettings>
```
Or you can init using code
```C#
var db = new AzureDb("your connection string");
```

## Usage
### Defining a model
```C#
 public class User : IDalObject
  {
      [DalAttribute("Id")]
      public int Id { get; set; }

      [DalAttribute("Email")]
      public string Email { get; set; }

      [DalAttribute("FirstName")]
      public string FirstName { get; set; }

      [DalAttribute("LastName")]
      public string LastName { get; set; }

      [DalAttribute("Password", typeof(PasswordCrypter))]
      public string Password { get; set; }
  }
```
DalAttribute allow the AzureDb object to detect what are the properties to bind. The string in the constructor is to find the right column name in the returns of your stored procedures for this model.
The PasswordCrypter object allows a password hash in order to encrypt your data. This object can't be decrypted ! It's a one way encryption

### Defining a procedure
***I won't be explaining what a stored procedure is. Google is your friend :)***
```C#
 public class GetUsers : IDbProcedure
  {
      public string Procedure
      {
          get
          {
              return "ps_User_Get"; //procedure name in the database
          }
      }
  }
```
This is a simple procedure configuration for a stored procedure without parameters

```C#
 public class GetUsersFromUserId : IDbProcedure
  {
      public string Procedure
      {
          get
          {
              return "ps_User_Get_FromUserId"; //procedure name in the database
          }
      }
    
    [DalAttribute("useId")] //procedure parameter
    public int UserId { get; set; }
  }
```
This is a simple procedure configuration for a stored procedure with a userId parameter

```C#
public class AddUser : IDbProcedure
  {
      public string Procedure
      {
          get
          {
              return "ps_User_Add"; //procedure name in the database
          }
      }

      [DalAttribute("firstName")]
      public string FirstName { get; set; }

      [DalAttribute("lastName")]
      public string LastName { get; set; }

      [DalAttribute("email")]
      public string Email { get; set; }

      [DalAttribute("login")]
      public string Login { get; set; }

      [DalAttribute("password", typeof(PasswordCrypter))]
      public string Password { get; set; }
    }
```
This is a simple procedure configuration for a stored procedure with a multiples parameters

Now let's see how to use this classes

### How does it work from here ?

```C#
  static void main(string[] args)
  {
    var db = new AzureDb("connection string"); //do not provide the parameter if you use configuration file
    
    var users = db.Execute<User>(new GetUsers());
    //this will execute the procedure "ps_User_Get" and return a list of User 
    
    var user = db.Execute<User>(new GetUsersFromUserId() { UserId = 1 }).FirstOrDefault();
    //This will execute the procedure ps_User_Get_FromUserId with the parameter userId set to 1
    //Execute<T> always return a list. It's up to you to know what to do with it. Here the logic wants you to take the first one
    
    db.ExecuteNonQuery(new AddUser() {
      FirstName = "HolyNoodle",
      LastName = "France",
      Email = "email@email.email",
      Login = "HolyNoodle",
      Password = "password"
    });
    //this will execute the ps_User_Add stored procedure with all the parameters
    //Not that the password will be encrypted before being send to the database
  }
```

## Advanced concept

### ProcedureBinder
The procedureBinder attribute allows you to bind a property of a model to a procedure in order to load the property from the procedure when you want

First of all, the procedure object to bind to a property 
```C#
  public class GetUserRightsFromId : IDbProcedure
  {
      public string Procedure
      {
          get
          {
              return "ps_UserRights_Get_FromId";
          }
      }

      [DalAttribute("id")]
      public int UserId { get; set; }
  }
```
Then the binding in the model
```C#
  public class User: IDalObject
    {
        [DalAttribute("Id")]
        public int Id { get; set; }
        
        [ProcedureBinder(BindedProcedure = typeof(GetUserRightsFromId), BindedProcedurePropertyName = "UserId", BindedSourcePropertyName = "Id", AutoBind = true)]
        public List<Right> RightList { get; set; }
    }
```
You are binding the property RightList to the procedure GetUserRightsFromId. The parameter of the procedure UserId is set to the value of the property Id in the User instance.
**The AutoBind property is set to false by default. If set to true, whenever the AzureDB object cross a ProcedureBinder with AutoBind true, it will execute the binded procedure. Care for the performance of your code**

You can manually refresh a binding by using this code 
```C#
static void main(string[] args)
{
  var db = new AzureDb("connection string"); //do not provide the parameter if you use configuration file
  var user = new User() 
  {
    Id = 1;
  };
  await db.RefreshBinding(user, "RightList");
}
```

### Validation
In order to validate procedure parameters you can implement IDbValidator on your procedure.
For example :
```C#
  public Tuple<bool, Dictionary<string, string>> Validate(AzureDb context)
  {
      var message = new Dictionary<string, string>();

      if (Login == null || Login == string.Empty)
      {
          message.Add("Login", "IsNull");
      }
      else
      {
          if (!CheckLength(Login))
          {
              message.Add("Login", "TooShort");
          }
          if (!CheckLoginCharacters(Login))
          {
              message.Add("Login", "UnwantedCharacters");
          }
      }
      if (Password == null || Password == string.Empty)
      {
          message.Add("Password", "IsNull");
      }
      else
      {
          if (!CheckLength(Password))
          {
              message.Add("Password", "TooShort");
          }
      }
      if (string.IsNullOrEmpty(Email))
      {
          message.Add("Email", "IsNull");
      }
      else
      {
          if(!CheckEmail(Email))
          {
              message.Add("Email", "WrongFormat");
          }
      }
      
      return new Tuple<bool, Dictionary<string, string>>(message.Count == 0, message);
  }
```
When the Dal validates to procedure, if there is errors an **AzureDbValidationException** will be throw with the data of the errors.

### Cache
**Warning : This feature hasn't been tested in every case.**

You can provide a cache provider using configuration file :
```xml
<appSettings>
  <add key="holynoodle:CacheProvider" value="HolyNoodle.Utility.DAL.BaseCacheProvider" />
</appSettings>
```
This line allows the DAL to use the **BaseCacheProvider**. In this line, the Dal expect a class implementing the **ICacheProvider** interface.

You can provide a custom cache provider.

You can bind procedures to clear others procedure cache. For example, you have a GetUsers procedures that returns the list of all users in your database. When you call AddUser, you need to uncache GetUsers in order to refresh datas.
```C#
  [CacheObject(typeof(GetUsers))]
  public class AddUser : IDbProcedure
```

# LocalisationHelper Documentation

This is quite simple

## The Files
create files according o this pattern language.*.json
The * must be replace by the language o the file
language.fr.json
language.en.json
language.ru.json
...

In this file the structure is a Dictionnary<string, string>
```json
{
  "LoginText": "this is the login form",
  "Logout": "please logout before exiting",
  "Home": "Home page",
  "Password": "Password",
  "Edit": "Edit",
  "Delete": "Delete",
}
```
**Important : This file copy local property must be set to always.**

**Important bis: This file is loaded at the runtime, so any changes done after the runtime will not be impacted since you rerun your code**

You have to init the LocalisationHelper.
In a website i put it in the Global.asax, for example :
```C#
protected void Application_Start()
{
    GlobalConfiguration.Configure(WebApiConfig.Register);
    FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
    RouteConfig.RegisterRoutes(RouteTable.Routes);
    BundleConfig.RegisterBundles(BundleTable.Bundles);

    HolyNoodle.Utility.LocalisationHelper.Init("en", HolyNoodle.Utility.ApplicationType.Web); //default language en
    //You can set the language files path using the parameter languageFileDirectory
    //HolyNoodle.Utility.LocalisationHelper.Init("en", HolyNoodle.Utility.ApplicationType.Web, languageFileDirectory: "path");
}
```
HolyNoodle.Utility.ApplicationType provide 2 values: Web (for websites, webapis and so on...) and StandAlone for executables (.exe).

The ChangeLanguage method allows you to change the user language
```C#
  HolyNoodle.Utility.LocalisationHelper.ChangeLanguage(new System.Globalization.CultureInfo("fr-fr"))
```

Finally, the GetText method give you the right translation for the key
```C#
  var translation = HolyNoodle.Utility.LocalisationHelper.GetText("Login");
```
