This extension provides an analyzer with code fix that gives you the opportunity to pull through documentation from a base class or interface member.  For example, if you have this interface:

```csharp

interface IMyInterface 
{
    /// <summary>
    /// This method does something
    /// </summary>
    void DoSomething();
}

```

And this class:

```csharp
class MyClass : IMyInterface
{
    public void DoSomething();
}
```

The code fix will allow you to pull through the documentation from the interface declaration to the class, resulting in this:

```csharp
class MyClass : IMyInterface
{
    /// <summary>
    /// This method does something
    /// </summary>
    public void DoSomething();
}
```

Alternatively, it will give the option to insert `<inheritdoc/>` instead (or even switch between the two):
```csharp
class MyClass : IMyInterface
{
    /// <inheritdoc/>
    public void DoSomething();
}
```

The diagnostic is hidden and will show up if you open the quick actions lightbulb when:
- Your cursor is on a member name
- One of the following is true:
  - The base member has documentation and the override member does not
  - The override member has `<summary>` documentation (giving you the option to switch to `<inheritdoc>`)
  - The override member has `<inheritdoc>` (giving you the option to switch to `<summary>`)

At the moment, only base classes and interfaces in your solution are supported.  Classes defined outside of your project will not pull through.
