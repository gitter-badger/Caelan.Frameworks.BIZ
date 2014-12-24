﻿namespace Caelan.Frameworks.BIZ.Classes

open System
open System.Linq
open System.Linq.Expressions
open System.Reflection
open Caelan.DynamicLinq.Classes
open Caelan.DynamicLinq.Extensions
open Caelan.Frameworks.Common.Classes
open Caelan.Frameworks.Common.Interfaces
open Caelan.Frameworks.BIZ.Interfaces

[<AbstractClass>]
[<AllowNullLiteral>]
type Repository(manager) = 
    
    interface IRepository with
        member this.GetUnitOfWork() = this.UnitOfWork
        member this.GetUnitOfWork<'T when 'T :> IUnitOfWork>() = this.UnitOfWork :?> 'T
    
    member private __.UnitOfWork : IUnitOfWork = manager
    member this.GetUnitOfWork() = (this :> IRepository).GetUnitOfWork()
    member this.GetUnitOfWork<'T when 'T :> IUnitOfWork>() = (this :> IRepository).GetUnitOfWork<'T>()

    static member Entity<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null>(manager : IUnitOfWork) =
        Repository<'TEntity>(manager)

and Repository<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null> internal (manager : IUnitOfWork) =
    member this.DTO<'TDTO when 'TDTO : equality and 'TDTO : null and 'TDTO : not struct>() =
        Repository<'TEntity, 'TDTO>(manager)

and [<AllowNullLiteral>] Repository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct>(manager) = 
    inherit Repository(manager : IUnitOfWork)
    
    interface IRepository<'TEntity, 'TDTO> with
        member __.DTOBuilder(mapper) = Builder.Source<'TEntity>().Destination<'TDTO>(mapper)
        member __.EntityBuilder(mapper) = Builder.Source<'TDTO>().Destination<'TEntity>(mapper)
        member __.DTOBuilder() = Builder.Source<'TEntity>().Destination<'TDTO>()
        member __.EntityBuilder() = Builder.Source<'TDTO>().Destination<'TEntity>()
        member __.Set() = manager.DbSet<'TEntity>()
        member this.SingleEntity([<ParamArray>] ids) = this.Set().Find(ids)
        
        member this.SingleEntity(expr) = 
            match expr with
            | null -> this.Set().FirstOrDefault()
            | _ -> this.Set().FirstOrDefault(expr)
        
        member this.SingleDTO([<ParamArray>] ids) = 
            match this.Set().Find(ids) with
            | null -> null
            | entity -> this.DTOBuilder().Build(entity)
        
        member this.SingleDTO(expr) = 
            let entity = 
                match expr with
                | null -> this.Set().FirstOrDefault()
                | _ -> this.Set().FirstOrDefault(expr)
            match entity with
            | null -> null
            | _ -> this.DTOBuilder().Build(entity)
        
        member this.List() = this.DTOBuilder().BuildList(this.All())
        member this.List whereExpr = this.DTOBuilder().BuildList(this.All(whereExpr))
        member this.All() = this.Set().AsQueryable()
        member this.All(take, skip, sort, filter, whereFunc) = 
            this.All(take, skip, sort, filter, whereFunc, this.DTOBuilder().BuildList)
        
        member this.All(whereExpr) = 
            match whereExpr with
            | null -> this.All()
            | _ -> this.Set().Where(whereExpr).AsQueryable()
        
        member this.Insert(dto : 'TDTO) = this.Set().Add(this.EntityBuilder().Build(dto)) |> ignore
        member this.Insert(entity : 'TEntity) = this.Set().Add(entity) |> ignore
        
        member this.Update(dto : 'TDTO, [<ParamArray>] ids) = 
            let entity = this.Set().Find(ids)
            let entry = manager.Entry(entity)
            this.EntityBuilder().Build(dto, ref entity)
            entry.CurrentValues.SetValues(entity)
        
        member this.Update(entity : 'TEntity, [<ParamArray>] ids) = 
            manager.Entry(this.Set().Find(ids)).CurrentValues.SetValues(entity)
        member this.Delete(_ : 'TDTO, [<ParamArray>] ids) = this.Set().Remove(this.Set().Find(ids)) |> ignore
        member this.Delete(_ : 'TEntity, [<ParamArray>] ids) = this.Set().Remove(this.Set().Find(ids)) |> ignore
        member this.Delete([<ParamArray>] ids : obj []) = this.Delete(this.SingleDTO(ids), ids)
    
    member val DTOMapper : IMapper<'TEntity, 'TDTO> = null with get, set
    member val EntityMapper : IMapper<'TDTO, 'TEntity> = null with get, set
    member this.DTOBuilder(mapper) = (this :> IRepository<'TEntity, 'TDTO>).DTOBuilder(mapper)
    member this.EntityBuilder(mapper) = (this :> IRepository<'TEntity, 'TDTO>).EntityBuilder(mapper)
    
    member this.DTOBuilder() = 
        match this.DTOMapper with
        | null -> (this :> IRepository<'TEntity, 'TDTO>).DTOBuilder()
        | _ -> this.DTOBuilder(this.DTOMapper)
    
    member this.EntityBuilder() = 
        match this.EntityMapper with
        | null -> (this :> IRepository<'TEntity, 'TDTO>).EntityBuilder()
        | _ -> this.EntityBuilder(this.EntityMapper)
    
    member this.Set() = (this :> IRepository<'TEntity, 'TDTO>).Set()
    member this.SingleEntity([<ParamArray>] ids : obj []) = (this :> IRepository<'TEntity, 'TDTO>).SingleEntity(ids)
    member this.SingleEntity(expr : Expression<Func<'TEntity, bool>>) = 
        (this :> IRepository<'TEntity, 'TDTO>).SingleEntity(expr)
    member this.SingleDTO([<ParamArray>] ids : obj []) = (this :> IRepository<'TEntity, 'TDTO>).SingleDTO(ids)
    member this.SingleDTO(expr : Expression<Func<'TEntity, bool>>) = 
        (this :> IRepository<'TEntity, 'TDTO>).SingleDTO(expr)
    member this.List() = (this :> IRepository<'TEntity, 'TDTO>).List()
    member this.List whereExpr = (this :> IRepository<'TEntity, 'TDTO>).List(whereExpr)
    member this.All() = (this :> IRepository<'TEntity, 'TDTO>).All()
    member this.All(whereExpr) = (this :> IRepository<'TEntity, 'TDTO>).All(whereExpr)
    
    member private this.All(take : int, skip : int, sort : seq<Sort>, filter : Filter, 
                            whereFunc : Expression<Func<'TEntity, bool>>, buildFunc : seq<'TEntity> -> seq<'TDTO>) = 
        let orderBy = 
            query { 
                for item in (typeof<'TEntity>).GetProperties(BindingFlags.Instance ||| BindingFlags.Public) 
                            |> Seq.map (fun t -> t.Name) do
                    select item
                    headOrDefault
            }
        
        let queryResult = 
            (match orderBy with
             | null -> this.All(whereFunc)
             | defaultSort -> this.All(whereFunc).OrderBy(defaultSort)).ToDataSourceResult(take, skip, sort, filter)
        
        DataSourceResult<'TDTO>(Data = buildFunc (queryResult.Data), Total = queryResult.Total)
    
    member this.All(take, skip, sort, filter, whereFunc) = 
        (this :> IRepository<'TEntity, 'TDTO>).All(take, skip, sort, filter, whereFunc)
    abstract Insert : dto:'TDTO -> unit
    abstract Insert : entity:'TEntity -> unit
    abstract Update : 'TDTO * ids:obj [] -> unit
    abstract Update : 'TEntity * ids:obj [] -> unit
    abstract Delete : 'TDTO * ids:obj [] -> unit
    abstract Delete : 'TEntity * ids:obj [] -> unit
    abstract Delete : ids:obj [] -> unit
    override this.Insert(dto : 'TDTO) = (this :> IRepository<'TEntity, 'TDTO>).Insert(dto)
    override this.Insert(entity : 'TEntity) = (this :> IRepository<'TEntity, 'TDTO>).Insert(entity)
    override this.Update(dto : 'TDTO, [<ParamArray>] ids) = (this :> IRepository<'TEntity, 'TDTO>).Update(dto, ids)
    override this.Update(entity : 'TEntity, [<ParamArray>] ids) = 
        (this :> IRepository<'TEntity, 'TDTO>).Update(entity, ids)
    override this.Delete(dto : 'TDTO, [<ParamArray>] ids) = (this :> IRepository<'TEntity, 'TDTO>).Delete(dto, ids)
    override this.Delete(entity : 'TEntity, [<ParamArray>] ids) = 
        (this :> IRepository<'TEntity, 'TDTO>).Delete(entity, ids)
    override this.Delete([<ParamArray>] ids : obj []) = (this :> IRepository<'TEntity, 'TDTO>).Delete(ids)
    member this.InsertAsync(dto : 'TDTO) = async { this.Insert(dto) } |> Async.StartAsTask
    member this.UpdateAsync(dto : 'TDTO, ids) = async { this.Update(dto, ids) } |> Async.StartAsTask
    member this.InsertAsync(entity : 'TEntity) = async { this.Insert(entity) } |> Async.StartAsTask
    member this.UpdateAsync(entity : 'TEntity, ids) = async { this.Update(entity, ids) } |> Async.StartAsTask
    member this.DeleteAsync(dto : 'TDTO, [<ParamArray>] ids) = async { this.Delete(dto, ids) } |> Async.StartAsTask
    member this.DeleteAsync(entity : 'TEntity, [<ParamArray>] ids) = 
        async { this.Delete(entity, ids) } |> Async.StartAsTask
    member this.DeleteAsync(ids : obj []) = async { this.Delete(ids) } |> Async.StartAsTask
    member this.ListAsync(whereExpr : Expression<Func<'TEntity, bool>>) = 
        async { return this.List(whereExpr) } |> Async.StartAsTask
    member this.ListAsync() = async { return this.List() } |> Async.StartAsTask
    member this.AllAsync(take : int, skip : int, sort : seq<Sort>, filter : Filter, 
                         whereFunc : Expression<Func<'TEntity, bool>>) = 
        async { return this.All(take, skip, sort, filter, whereFunc) } |> Async.StartAsTask
    member this.SingleDTOAsync([<ParamArray>] id : obj []) = async { return this.SingleDTO(id) } |> Async.StartAsTask
    member this.SingleDTOAsync(expr : 'TEntity -> bool) = async { return this.SingleDTO(expr) } |> Async.StartAsTask
    member this.SingleEntityAsync([<ParamArray>] id : obj []) = 
        async { return this.SingleEntity(id) } |> Async.StartAsTask
    member this.SingleEntityAsync(expr : 'TEntity -> bool) = 
        async { return this.SingleEntity(expr) } |> Async.StartAsTask