﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JCE.Applications.Aspects;
using JCE.Applications.Dtos;
using JCE.Datas.Queries;
using JCE.Validations.Aspects;

namespace JCE.Applications
{
    /// <summary>
    /// 增删改查服务
    /// </summary>
    /// <typeparam name="TDto">数据传输对象类型</typeparam>
    /// <typeparam name="TQueryParameter">查询参数类型</typeparam>
    public interface ICrudService<TDto, in TQueryParameter> : ICrudService<TDto, TDto, TQueryParameter>
        where TDto : IDto, new() 
        where TQueryParameter : IQueryParameter
    {        
    }

    /// <summary>
    /// 增删改查服务
    /// </summary>
    /// <typeparam name="TDto">数据传输对象类型</typeparam>
    /// <typeparam name="TRequest">请求参数类型</typeparam>
    /// <typeparam name="TQueryParameter">查询参数类型</typeparam>
    public interface ICrudService<TDto, TRequest, in TQueryParameter> : IQueryService<TDto, TQueryParameter>
        where TDto : IDto, new() 
        where TRequest : IRequest, new() 
        where TQueryParameter : IQueryParameter
    {
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request">请求参数</param>
        [UnitOfWork]
        void Save([Valid] TRequest request);

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request">请求参数</param>
        /// <returns></returns>
        Task SaveAsync([Valid] TRequest request);

        /// <summary>
        /// 批量保存
        /// </summary>
        /// <param name="addList">新增列表</param>
        /// <param name="updateList">修改列表</param>
        /// <param name="deleteList">删除列表</param>
        /// <returns></returns>
        List<TDto> Save(List<TRequest> addList, List<TRequest> updateList, List<TRequest> deleteList);

        /// <summary>
        /// 批量保存
        /// </summary>
        /// <param name="addList">新增列表</param>
        /// <param name="updateList">修改列表</param>
        /// <param name="deleteList">删除列表</param>
        /// <returns></returns>
        Task<List<TDto>> SaveAsync(List<TRequest> addList, List<TRequest> updateList, List<TRequest> deleteList);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="ids">用逗号分隔的Id列表，范例："1,2"</param>
        void Delete(string ids);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="ids">用逗号分隔的Id列表，范例："1,2"</param>
        /// <returns></returns>
        Task DeleteAsync(string ids);
    }
}
