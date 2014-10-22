﻿module Caelan.Frameworks.BIZ.Classes.GenericRepository 

open System
open System.Linq
open System.Reflection
open Caelan.Frameworks.BIZ.Interfaces

let findRepository ([<ParamArray>] args : obj []) (baseType, baseGenericType : Type) = 
    let findRepository (assembly : Assembly) = 
        match assembly with
        | null -> None
        | _ -> 
            match assembly.GetTypes() |> Seq.tryFind (fun t -> t.BaseType = baseType || baseType.IsAssignableFrom(t)) with
            | Some(repoType) -> Some(Activator.CreateInstance(repoType, args))
            | None -> None
    
    let rec getRepository asmList = 
        match asmList with
        | head :: tail -> 
            match head |> findRepository with
            | Some(repo) -> repo
            | None -> tail |> getRepository
        | [] -> 
            match baseType.IsGenericTypeDefinition with
             | true -> Activator.CreateInstance(baseGenericType)
             | _ -> Activator.CreateInstance(baseType, args)
    
    [ Assembly.GetExecutingAssembly()
      Assembly.GetEntryAssembly()
      Assembly.GetCallingAssembly() ]
    |> getRepository

let CreateGenericRepository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct>(manager : IUnitOfWork) = 
    let baseType = typeof<IRepository<'TEntity, 'TDTO>>
    (baseType, baseType.MakeGenericType(typeof<'TEntity>, typeof<'TDTO>)) |> findRepository [| manager |] :?> IRepository<'TEntity, 'TDTO>

let CreateGenericListRepository<'TEntity, 'TDTO, 'TListDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct and 'TListDTO : equality and 'TListDTO : null and 'TListDTO : not struct>(manager : IUnitOfWork) = 
    let baseType = typeof<IListRepository<'TEntity, 'TDTO, 'TListDTO>>
    (baseType, baseType.MakeGenericType(typeof<'TEntity>, typeof<'TDTO>, typeof<'TListDTO>)) 
    |> findRepository [| manager |] :?> IListRepository<'TEntity, 'TDTO, 'TListDTO>