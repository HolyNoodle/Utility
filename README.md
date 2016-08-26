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
    var db = new AzureDb("connection string");
    
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
  var db = new AzureDb("connection string");
  var user = new User() 
  {
    Id = 1;
  };
  await db.RefreshBinding(user, "RightList");
}
```


