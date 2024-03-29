This analyzer provides a code fix that gives you the opportunity to pull through documentation from a base class or interface member.  For example, if you have this interface:

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

Finally, it also offers the ability to "promote" documentation to a base class member, in the event that the derived member is correct.  For example, before:
```csharp

class BaseClass
{
    /// <summary>
    /// This doc is wrong
    /// </summary>
    public virtual void DoSomething() {}
}

class Derived : BaseClass
{
    /// <summary>
    /// This doc is right
    /// </summary>
    public override void DoSomething() {}
}

```

After:
```csharp

class BaseClass
{
    /// <summary>
    /// This doc is right
    /// </summary>
    public virtual void DoSomething() {}
}

class Derived : BaseClass
{
    /// <inheritdoc />
    public override void DoSomething() {}
}

```

The diagnostic is hidden and will show up if you open the quick actions lightbulb when:
- Your cursor is on a member name
- One of the following is true:
  - The base member (see below) has documentation and the override member does not
  - The override member has `<summary>` documentation (giving you the option to switch to `<inheritdoc>`)
  - The override member has `<inheritdoc>` (giving you the option to switch to `<summary>`)
  - The override member has `<summary>` documentation and it is different than the base member.  This will give you the opportunity to promote the docs to the base member.

The "base member" can be located in
- A class in the same solution, like `MyClass.BaseMember()` (this works the best as the documentation is available in the source code)
- An external library, like `Object.ToString()`.  This should mostly work, but does have some limitations and caveats - see [this issue](https://github.com/someguy20336/PullThroughDoc/issues/12) if you are having problems with the analyzer/code fix.  If you don't think your problem falls into any of the caveats outlined, submit a new issue.
  - Note: For the case in which you are trying to promote the documentation, external libraries are not supported.

## Installation

You can install one of two ways
- [Visual Studio extension](https://marketplace.visualstudio.com/items?itemName=someguy20336.PullThroughDoc)
  - This will make it available for all projects
- [Nuget Package](https://www.nuget.org/packages/PullThroughDoc/) (Package Name: `PullThroughDoc`)
  - This will make it available for the specific project you installed it on

## Diagnostics Added
The following diagnostics are provided (with code fixes)
- `PullThroughDoc01` - Gives you the option to insert the `<summary>` tag from the base member to the overridden member, or just use `<inheritdoc>`
- `PullThroughDoc02` - Swaps a `<summary>` documentation with `<inheritdoc>`
- `PullThroughDoc03` - Swaps `<inheritdoc>` to `<summary>` from the base member
- `PullThroughDoc04` - Promotes the `<summary>` documentation from the override member to the base member and inserts `<inheritdoc>` in it's place. The base member must exist in the source code of the solution.

If you would like to disable any of these, use the [editor config](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-options#severity-level).  Example:
```
dotnet_diagnostic.PullThroughDoc04.severity = none
```
