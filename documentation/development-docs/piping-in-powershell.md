# Azure PowerShell Piping Scenarios

There are two main scenarios that we wish to enable in cmdlets for Azure PowerShell:
- piping by value using an `InputObject` parameter
- piping by property name using a `ResourceId` parameter

## Using the `InputObject` parameter

### Short explanation
For all resources, `InputObject` should be implemented for the Remove/Set/Update cmdlets (and any other cmdlet where an existing resource is being operated on). The implementation of this will be a new parameter set:

```powershell
Remove/Set/Update-Az<ExampleResource> -InputObject <TypeOfExampleResource> <Other parameters that cannot be obtained from TypeOfExampleResource>
```

For all child resources, `InputObject` should also be implemented for the Get/New cmdlets.  The implementation of this will be a new parameter set:

```powershell
Get/New-Az<ExampleResource> -ParentObject <TypeOfParentOfExampleResource> <Other parameters that cannot be obtained from TypeOfParentOfExampleResource>
```

### Long explanation
This scenario should be used when piping objects around within the same module. For example, if you have a set of `Get-AzFoo`, `Remove-AzFoo`, and `Set-AzFoo` cmdlets, the `Remove-AzFoo` and `Set-AzFoo` cmdlets should have a parameter set that takes the `InputObject` parameter of type `PSFoo` so that a user can do the following:

```powershell
# --- Piping scenario ---
# Setting and removing an individual object
Get-AzFoo -Name "FooName" -ResourceGroupName "RG" | Set-AzFoo <additional parameters>
Get-AzFoo -Name "FooName" -ResourceGroupName "RG" | Remove-AzFoo

# Setting and removing a collection of objects
Get-AzFoo | Set-AzFoo <additional parameters>
Get-AzFoo | Remove-AzFoo


# --- Non-piping scenario ---
# Setting and removing an individual object
$foo = Get-AzFoo -Name "FooName" -ResourceGroupName "RG"
Set-AzFoo -InputObject $foo <additional parameters>
Remove-AzFoo -InputObject $foo

# Setting and removing a collection of objects
Get-AzFoo | ForEach-Object { Set-AzFoo -InputObject $_ <additional parameters> }
Get-AzFoo | ForEach-Object { Remove-AzFoo -InputObject $_ }
```

Another time that this scenario applies is when you have cmdlets for child resources that need information about the parent (top-level) resource. For example you can pipe in the whole parent object to the `New-AzFooBar` and `Get-AzFooBar` cmdlets to get the child resources, and then pipe the child resource object to the `Remove-AzFooBar` and `Set-AzFooBar` cmdlets.

```powershell
# --- Piping scenario ---
# Getting all of child resources from all of the parent resources and removing them
Get-AzFoo | Get-AzFooBar | Remove-AzFooBar

# Getting all of the child resources from all of the parent resources in a resource group and removing them
Get-AzFoo -ResourceGroupName "RG" | Get-AzFooBar | Remove-AzFooBar

# Getting all of the child resources from a specific parent resource and removing them
Get-AzFoo -ResourceGroupName "RG" -Name "FooName" | Get-AzFooBar | Remove-AzFooBar


# --- Non-piping scenario ---
# Getting all of the child resources from a specific parent resource and removing them
$foo = Get-AzFoo -ResourceGroupName "RG" -Name "FooName"
$fooBar = Get-AzFooBar -InputObject $foo
Remove-AzFooBar -InputObject $fooBar
```

## Using the `ResourceId` parameter

### Short explanation
For all resources, `ResourceId` should be implemented for the Remove/Set/Update cmdlets (and any other cmdlet where an existing resource is being operated on). The implementation of this will be a new parameter set:

```powershell
Remove/Set/Update-Az<ExampleResource> -ResourceId <string (accepts ExampleResource ResourceId> <Other parameters that cannot be obtained from ExampleResource ResourceId>
```

For all child resources, `ResourceId` should also be implemented for the Get/New cmdlets.  The implementation of this will be a new parameter set:

```powershell
Get/New-Az<ExampleResource> -ParentResourceId <string (accepts ResourceId of the parent of ExampleResource> <Other parameters that cannot be obtained from the ResourceId of the parent of ExampleResource>
```

### Long explanation

In this scenario, we are using the generic cmdlets found in the `Az.Resources` module. These cmdlets, `Find-AzResource` and `Get-AzResource`, return a `PSCustomObject` that has a `ResourceId` property, which is the unique identifier for the given resource. Since this identifier can parsed to get the name and resource group name for a top-level resource, we can create a parameter set that has a `ResourceId` parameter that accepts its value from the pipeline by property name, allowing us to accept piping from these generic cmdlets.

```powershell
# --- Piping scenario ---
# Remove all Foo objects in the current subscription
Find-AzResource -ResourceType Microsoft.Foo/foo | Remove-AzFoo

# Remove all Foo objects in a given resource group
Find-AzResource -ResourceType Microsoft.Foo/foo -ResourceGroupEquals "RG" | Remove-AzFoo

# Remove a specific Foo object
Find-AzResource -ResourceGroupEquals "RG" -ResourceNameEquals "FooName" | Remove-AzFoo


# -- Non-piping scenario ---
# Removing all Foo objects in the current subscription
Find-AzResource -ResourceType Microsoft.Foo/foo | ForEach-Object { Remove-AzFoo -ResourceId $_.ResourceId }

# Remove all Foo objects in a given resource group
Find-AzResource -ResourceType Microsoft.Foo/foo -ResourceGroupEquals "RG" | ForEach-Object { Remove-AzFoo -ResourceId $_.ResourceId }

# Remove a specific Foo object
Find-AzResource -ResourceGroupEquals "RG" -ResourceNameEquals "FooName" | ForEach-Object { Remove-AzFoo -ResourceId $_.ResourceId }
```

To implement this scenario, please see the [`ResourceIdentifier`](https://github.com/Azure/azure-powershell-common/blob/master/src/ResourceManager/Version2016_09_01/Utilities/Models/ResourceIdentifier.cs) class in the `ResourceManager` project. This class will allow you to create a `ResourceIdentifier` object that accepts a `ResourceId` string in its constructor and has properties `ResourceName`, `ResourceGroupName`, and others.

## Summary
For all Remove/Set/Update cmdlets (and any other cmdlet where an existing resource is being operated on), you will have three parameter sets (and potentially a multiple of three if you have initially have multiple parameter sets):
```powershell
Remove/Set/Update-Az<ExampleResource> <parameters that are required to identify ExampleResource> <Other parameters>
Remove/Set/Update-Az<ExampleResource> -InputObject <TypeOfExampleResource> <Other parameters that cannot be obtained from TypeOfExampleResource>
Remove/Set/Update-Az<ExampleResource> -ResourceId <string (accepts ExampleResource ResourceId> <Other parameters that cannot be obtained from ExampleResource ResourceId>
```
For example, for child resource "Widget" with parent "Foo", there will be these three parameter sets for Remove:
```powershell
Remove-AzWidget -ResourceGroupName <string> -FooName <string> -Name <string>
Remove-AzWidget -InputObject <Widget>
Remove-AzWidget -ResourceId <string>
```
For all child resources, Get/New cmdlets will also have these three parameter sets:
```powershell
Get/New-Az<ExampleResource> <parameters needed to identify/create ExampleResource>
Get-Az<ExampleResource> -InputObject <TypeOfParentOfExampleResource> <Other parameters that cannot be obtained from TypeOfParentOfExampleResource>
Get/New-Az<ExampleResource> -ResourceId <string (accepts ResourceId of the parent of ExampleResource> <Other parameters that cannot be obtained from the ResourceId of the parent of ExampleResource>
```
For example, for child resource "Widget" with parent "Foo", there will be these three parameter sets for New:
```powershell
New-AzWidget -ResourceGroupName <string> -FooName <String> -Name <string>
New-AzWidget -WidgetObject <Foo> -Name <string>
New-AzWidget -WidgetResourceId <string> -Name <string>
```

# Piping in PowerShell (additional information)

## Understanding Piping

In PowerShell, cmdlets pipe objects between one another; cmdlets should return objects, not text (strings).

For example, in Azure PowerShell, you can remove all of your current environments with the following pipeline scenairo:

```powershell
Get-AzEnvironment | Remove-AzEnviornment
```

The cmdlet `Get-AzEnvironment` will return a set of `Environment` objects, and those objects will be individually piped to the `Remove-AzEnvironment` cmdlet, where they will be removed.

When an object is being piped to a cmdlet, PowerShell will first check to see if it can bind that object to a parameter with the same type and has the property `ValueFromPipeline = true`. If this cannot be done, PowerShell will then see if it can bind the properties of the object with parameters that share the same name and have the property `ValueFromPipelineByPropertyName = true`.

### ValueFromPipeline

In this scenario, the object piped to the cmdlet will be bound to a parameter with the same type that has the property `ValueFromPipeline = true`.

For example, you have a `Remove-AzFoo` cmdlet with the following parameter:

```cs
[Parameter(ParameterSetName = "ByInputObject", ValueFromPipeline = true)]
public PSFoo InputObject { get; set; }
```

If there is a corresponding `Get-AzFoo` cmdlet that returns a `PSFoo` object, the following scenario is enabled:

```powershell
# --- Piping scenario ---
# Remove an individual PSFoo object
Get-AzFoo -Name "FooName" -ResourceGroupName "RG" | Remove-AzFoo

# Remove all PSFoo objects
Get-AzFoo | Remove-AzFoo


# --- Non-piping scenarios ---
# Remove an individual PSFoo object
$foo = Get-AzFoo -Name "FooName" -ResourceGroupName "RG"
Remove-AzFoo -InputObject $foo

# Remove all PSFoo objects
Get-AzFoo | ForEach-Object { Remove-AzFoo -InputObject $_ }
```

The `PSFoo` object(s) that is returned by the `Get-AzFoo` call will be piped to the `Remove-AzFoo` cmdlet, and becuase that cmdlet has a parameter that accepts a `PSFoo` object by value, PowerShell will bind the object being sent through the pipeline to this `InputObject` parameter.

### ValueFromPipelineByPropertyName

In this scenario, the properties of the object being piped to the cmdlet will be bound to parameters with the same name that have the property `ValueFromPipelineByPropertyName = true`.

For example, you have a `Remove-AzFoo` cmdlet with the following parameters:

```cs
[Parameter(ParameterSetName = "ByPropertyName", ValueFromPipelineByPropertyName = true)]
public string ResourceId { get; set; }
```

If there is a corresponding `Get-AzFoo` cmdlet that returns a `PSFoo` object (that has properties `Name` and `ResourceGroupName`), the following scenario is enabled:

```powershell
# --- Piping scenario ---
# Remove an individual PSFoo object
Get-AzResource -ResourceId <resourceId> | Remove-AzFoo

# Remove all PSFoo objects
Get-AzResource -ResourceType Foo | Remove-AzFoo


# --- Non-piping scenario ---
# Remove an individual PSFoo object
$foo = Get-AzResource -ResourceId <resourceId>
Remove-AzFoo -ResourceId <resourceId>

# Remove all PSFoo objects
Get-AzFoo | ForEach-Object { Remove-AzFoo -ResourceId <resourceId> }
```

The `PSFoo` object(s) that is returned by the `Get-AzFoo` call will be piped to the `Remove-AzFoo` cmdlet, and because that cmdlet has parameters that accept their value from the pipeline by property name, PowerShell will check each of these parameters and see if it can find a corresponding property in the `PSFoo` object that it shares a name with, and bind the value.

### Writing to the Pipeline

To write to the pipeline, use the `WriteObject` method from the `AzurePSCmdlet` class in the `Commands.Common` project. If you are writing **only one** object to the pipeline, then you should use the overload `WriteObject(object sendToPipeline)`, but if you are writing **more than one** objects to the pipeline, you should use the overload `WriteObject(object sendToPipeline, bool enumerateCollection)`.

If you use the `WriteObject(object sendToPipeline)` overload when writing a collection of objects to the pipeline, this will cause errors in the piping scenario, specifically, it will pipe a collection to the next cmdlet, rather than the individual objects.

```cs
public override void ExecuteCmdlet()
{
    if (!string.IsNullOrEmpty(this.Name))
    {
        // If the Name is not empty, write a single object to the pipeline
        var result = client.Foo.List(this.Name);
        WriteObject(result);
    }
    else
    {
        // If the Name is empty, write a collection of objects to the pipeline
        var result = client.Foo.List();
        WriteObject(result, true);
    }
}
```

## More Information

For more information on piping, see the article ["Understanding the Windows PowerShell Pipeline"](https://msdn.microsoft.com/en-us/powershell/scripting/getting-started/fundamental/understanding-the-windows-powershell-pipeline).
