﻿namespace Caelan.Frameworks.BIZ.Interfaces

open System
open System.Data.Entity
open System.Data.Entity.Infrastructure
open System.Linq
open System.Linq.Expressions
open System.Collections.Generic
open Caelan.DynamicLinq.Classes

type IRepository =     
    abstract UnitOfWork : IUnitOfWork

and IRepository<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null> = 
    inherit IRepository
    abstract Set : DbSet<'TEntity>
    abstract SingleEntity : [<ParamArray>]ids:obj [] -> 'TEntity
    abstract SingleEntity : where:Expression<Func<'TEntity, bool>> -> 'TEntity
    abstract All : unit -> IQueryable<'TEntity>
    abstract All : where:Expression<Func<'TEntity, bool>> -> IQueryable<'TEntity>
    abstract Insert : entity:'TEntity -> 'TEntity
    abstract Update : entity:'TEntity * [<ParamArray>]ids:obj [] -> unit
    abstract Delete : [<ParamArray>]ids:obj [] -> unit

and IRepository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct> = 
    inherit IRepository<'TEntity>
    abstract SingleDTO : [<ParamArray>]ids:obj [] -> 'TDTO
    abstract SingleDTO : where:Expression<Func<'TEntity, bool>> -> 'TDTO
    abstract List : unit -> seq<'TDTO>
    abstract List : Expression<Func<'TEntity, bool>> -> seq<'TDTO>
    abstract All : take:int * skip:int * sort:ICollection<Sort> * filter:Filter * where:Expression<Func<'TEntity, bool>>
     -> DataSourceResult<'TDTO>
    abstract Insert : dto:'TDTO -> 'TDTO
    abstract Update : dto:'TDTO * [<ParamArray>]ids:obj [] -> unit

and IListRepository<'TEntity, 'TDTO, 'TListDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct and 'TListDTO : equality and 'TListDTO : null and 'TListDTO : not struct> = 
    inherit IRepository<'TEntity, 'TDTO>
    abstract ListRepository : IRepository<'TEntity, 'TListDTO> with get, set

and IUnitOfWork = 
    inherit IDisposable
    abstract SaveChanges : unit -> int
    abstract Entry<'TEntity> : entity:'TEntity -> DbEntityEntry
    abstract DbSet<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null> : unit
     -> DbSet<'TEntity>
    abstract Transaction : body:Action<IUnitOfWork> -> unit
    abstract TransactionSaveChanges : body:Action<IUnitOfWork> -> bool
    abstract CustomRepository<'TRepository when 'TRepository :> IRepository> : unit -> 'TRepository
    abstract Repository<'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null> : unit
     -> IRepository<'TEntity>
    abstract Repository<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct> : unit
     -> IRepository<'TEntity, 'TDTO>
    abstract Repository<'TEntity, 'TDTO, 'TListDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : equality and 'TDTO : null and 'TDTO : not struct and 'TListDTO : equality and 'TListDTO : null and 'TListDTO : not struct> : unit
     -> IListRepository<'TEntity, 'TDTO, 'TListDTO>

type IUnitOfWorkCaller = 
    abstract UnitOfWork<'T> : call:Func<IUnitOfWork, 'T> -> 'T
    abstract UnitOfWork : call:Action<IUnitOfWork> -> unit
    abstract CustomRepository<'T, 'TRepository when 'TRepository :> IRepository> : call:Func<'TRepository, 'T> -> 'T
    abstract Repository<'T, 'TEntity when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null> : call:Func<IRepository<'TEntity>, 'T>
     -> 'T
    abstract Repository<'T, 'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : not struct and 'TDTO : equality and 'TDTO : null> : call:Func<IRepository<'TEntity, 'TDTO>, 'T>
     -> 'T
    abstract RepositoryList<'TEntity, 'TDTO when 'TEntity : not struct and 'TEntity : equality and 'TEntity : null and 'TDTO : not struct and 'TDTO : equality and 'TDTO : null> : unit
     -> seq<'TDTO>
    abstract UnitOfWorkSaveChanges : call:Action<IUnitOfWork> -> bool
    abstract Transaction : body:Action<IUnitOfWork> -> unit
    abstract TransactionSaveChanges : body:Action<IUnitOfWork> -> bool
