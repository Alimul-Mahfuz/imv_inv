# Implementing Laravel-style Observers in ASP.NET Core with EF Core Interceptors

If you're coming from Laravel, you're probably familiar with **Observers**—a clean way to hook into model lifecycle events (like `created`, `updated`) to handle side effects like sending emails or updating search indices.

In ASP.NET Core, we can achieve this same level of decoupling using **Domain Events** and **EF Core Interceptors**.

## 🚀 How it Works

Instead of bloating your Controllers with side-effect logic, the workflow looks like this:
1. **Trigger an Event**: In your Controller/Service, simply add an event to your Entity.
2. **Save to Database**: Call `_context.SaveChangesAsync()`.
3. **Auto-Dispatch**: An EF Core Interceptor automatically picks up the event and runs the corresponding "Observer" (Handler) **only if the database save was successful**.

---

## 🛠️ The Setup

### 1. The Entity (The Model)
Your entity inherits from a `BaseEntity` that tracks events.

```csharp
public class User : BaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    // ...
}
```

### 2. The Event
A simple class representing what happened.

```csharp
public class UserRegisteredEvent : IDomainEvent
{
    public User User { get; }
    public UserRegisteredEvent(User user) => User = user;
}
```

### 3. The Observer (The Handler)
This is where the side-effect logic lives. No more clutter in the Controller!

```csharp
public class UserObserver : IDomainEventHandler<UserRegisteredEvent>
{
    private readonly IEmailService _emailService;

    public UserObserver(IEmailService emailService) => _emailService = emailService;

    public async Task Handle(UserRegisteredEvent domainEvent)
    {
        // Send Welcome Email
        await _emailService.SendTemplateEmailAsync(...);
    }
}
```

### 4. Triggering the Event
In your `AuthController`, you just state your intent.

```csharp
[HttpPost]
public async Task<IActionResult> DoRegister(CreateUserViewModel model)
{
    var user = new User { Name = model.Name, ... };
    
    // Register the "Created" event
    user.AddDomainEvent(new UserRegisteredEvent(user));

    _dbContext.Add(user);
    await _dbContext.SaveChangesAsync(); // Interceptor fires here!

    return RedirectToAction("Welcome");
}
```

---

## 💎 Why use this pattern?

✅ **Clean Controllers**: Controllers focus on HTTP concerns, not business side-effects.
✅ **Atomicity**: Events only fire if the database transaction succeeds.
✅ **Extensibility**: Want to add a Slack notification or log to an external API? Just create a new Handler—zero changes to the existing registration code.
✅ **Testability**: You can unit test your Observers in isolation.

---

## 🏗️ Technical implementation details
We use a **`SaveChangesInterceptor`** to scan the EF Core Change Tracker for any entity implementing `IHasDomainEvents`. This ensures that we don't dispatch events unless the data is safely persisted.

---

### Want to see the code? 
Check out the `Events`, `Observers`, and `Interceptors` folders in this project!
