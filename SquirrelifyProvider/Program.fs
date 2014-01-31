namespace SquirrelifyProvider

open System
open System.IO
open System.Reflection
open System.Collections.Generic
open Samples.FSharp.ProvidedTypes

open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Quotations

[<TypeProvider>]
type Provider(config: TypeProviderConfig) as this = 
    inherit TypeProviderForNamespaces()

    let ns = "SquirrelifyProvider"
    let asm = Assembly.GetExecutingAssembly()

    let rnd = Random(DateTime.Now.Millisecond)

    let createTypes() =        
        let typeDict = Dictionary<string, ProvidedTypeDefinition>()        
        let megaType = ProvidedTypeDefinition("ItsSquirrelsAndOtherStuffAllTheWayDown",None,HideObjectMethods=true)
         
         
        let rec recusrsiveTypesAreRecursive() =       
            let nextType = ProvidedTypeDefinition(Guid.NewGuid().ToString() + " Squirrel",None,HideObjectMethods=true)            
            nextType.AddMembersDelayed( fun _ ->                
               let nextNextType = recusrsiveTypesAreRecursive()
               megaType.AddMember nextNextType
               let prop = ProvidedProperty("Squirrelify!",nextNextType,GetterCode = fun _ -> <@@ obj() @@> )
               prop.AddXmlDocDelayed( fun () -> 
                  
                  let ascii = 
                     Data.ascii.[rnd.Next(0,Data.ascii.Length)]. Split([|"\n"|],System.StringSplitOptions.None)                     
                     // note the next line is replacing spaces with char 255 which looks like a space but isn't one!
                     // otherwise the xml comments gobble up all the spaces!
                     |> Array.map(fun line -> "<para>" + line.Replace(" "," ").Replace("&","&amp;").Replace("<","&lt;").Replace(">","&gt;").Replace("'","&apos;").Replace("\"","&quot;") + "</para>")
                     |> fun data -> String.Join("",data)
                  "<summary>" + ascii + "</summary>" 
                  
                  )
               [prop:>MemberInfo;recusrsiveTypesAreRecursive():>MemberInfo] )
            nextType


        megaType.AddMemberDelayed( fun () ->  
         let nextType = recusrsiveTypesAreRecursive()
         megaType.AddMember(nextType)
         let prop = ProvidedProperty("Squirrelify!",nextType,GetterCode = fun _ -> <@@ obj() @@> )
         prop.AddXmlDocDelayed( fun () -> "Begin the squirrelification process..." )
         prop
         
        )
         
       
        megaType
        
         
    let rootType = ProvidedTypeDefinition(asm, ns, "SquirrelifyProvider", None, HideObjectMethods = true)
    
    do 
      rootType.AddMember(ProvidedConstructor([],InvokeCode = (fun _ -> <@@ obj() @@> )))
      let unicornType = createTypes()
      rootType.AddMember(ProvidedMethod("Create",[],unicornType, InvokeCode = (fun _ -> <@@ obj() @@> ), IsStaticMethod = true))
      rootType.AddMember unicornType
      this.AddNamespace(ns, [rootType])


[<assembly:TypeProviderAssembly>] 
do()