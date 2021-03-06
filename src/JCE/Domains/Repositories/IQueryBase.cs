﻿using System;
using System.Collections.Generic;
using System.Text;

namespace JCE.Domains.Repositories
{
    /// <summary>
    /// 查询对象
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    public interface IQueryBase<TEntity>:ICriteria<TEntity> where TEntity:class
    {
        /// <summary>
        /// 获取排序条件
        /// </summary>
        /// <returns></returns>
        string GetOrder();

        /// <summary>
        /// 获取分页参数
        /// </summary>
        /// <returns></returns>
        IPager GetPager();
    }
}
