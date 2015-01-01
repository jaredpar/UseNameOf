UseNameOf
====

This is a tool for converting legacy CSharp code bases to use the `nameof` expression instead of string literals.  

**Before**

``` csharp
void M(string text) 
{
  if (text == null) 
  {
    throw new ArgumentNullException("text");
  }
  ...
}
```

**After**

``` csharp
void M(string text) 
{
  if (text == null) 
  {
    throw new ArgumentNullException(nameof(text));
  }
  ...
}
```

It will only do this transformation when it knows it is correct.  Passing in bad names will not result in a conversion

``` csharp
void G(int p)
{
  // "b" isn't a parameter, won't be transformed
  throw new ArgumentException("b");
}
```
