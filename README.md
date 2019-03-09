This extension provides an analyzer with code fix that gives you the opportunity to pull through documentation from a base class or interface member.  For example, if you have this interface:

```C#

interface IMyInterface 
{
    /// <summary>
    /// This method does something
    /// </summary>
    void DoSomething();
}

```

And this class:

```C#
class MyClass : IMyInterface
{
    public void DoSomething();
}
```

The code fix will allow you to pull through the documentation from the interface declaration to the class, resulting in this:

```C#
class MyClass : IMyInterface
{
    /// <summary>
    /// This method does something
    /// </summary>
    public void DoSomething();
}
```

At the moment, only base classes and interfaces in your solution are supported.  Classes defined outside of your project will not pull through.
