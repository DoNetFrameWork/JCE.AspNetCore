﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using JCE.Datas.EntityFramework.Configs;
using JCE.Datas.EntityFramework.Core;
using JCE.Logs;
using JCE.Logs.Extensions;
using JCE.Utils.Extensions;
using JCE.Utils.Helpers;
using Microsoft.Extensions.Logging;

namespace JCE.Datas.EntityFramework.Logs
{
    /// <summary>
    /// EF日志记录器
    /// </summary>
    public class EfLog:ILogger
    {
        /// <summary>
        /// EF跟踪日志名
        /// </summary>
        public const string TraceLogName = "EfTraceLog";

        /// <summary>
        /// 日志操作
        /// </summary>
        private readonly ILog _log;

        /// <summary>
        /// 工作单元
        /// </summary>
        private readonly UnitOfWorkBase _unitOfWork;

        /// <summary>
        /// 日志分类
        /// </summary>
        private readonly string _category;

        /// <summary>
        /// 初始化一个<see cref="EfLog"/>类型的实例
        /// </summary>
        /// <param name="log">日志操作</param>
        /// <param name="unitOfWork">工作单元</param>
        /// <param name="category">日志分类</param>
        public EfLog(ILog log, UnitOfWorkBase unitOfWork, string category)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _category = category;
        }

        /// <summary>
        /// 日志记录
        /// </summary>
        /// <typeparam name="TState">状态类型</typeparam>
        /// <param name="logLevel">日志级别</param>
        /// <param name="eventId">事件编号</param>
        /// <param name="state">状态</param>
        /// <param name="exception">异常</param>
        /// <param name="formatter">日志内容</param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (IsEnabled(eventId) == false)
            {
                return;
            }
            _log.Caption($"执行EF操作：{_category}")
                .Content($"工作单元跟踪号：{_unitOfWork.TraceId}")
                .Content($"事件Id：{eventId.Id}")
                .Content($"事件名称：{eventId.Name}");
            AddContent(state);
            _log.Exception(exception).Trace();
        }

        /// <summary>
        /// 是否启用Ef日志
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <returns></returns>
        private bool IsEnabled(EventId eventId)
        {
            if (EfConfig.LogLevel == EfLogLevel.Off)
            {
                return false;
            }
            if (EfConfig.LogLevel == EfLogLevel.All)
            {
                return true;
            }
            if (eventId.Name == "Microsoft.EntityFrameworkCore.Database.Command.CommandExecuted")
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 添加日志内容
        /// </summary>
        /// <typeparam name="TState">状态类型</typeparam>
        /// <param name="state">状态</param>
        private void AddContent<TState>(TState state)
        {
            if (EfConfig.LogLevel == EfLogLevel.All)
            {
                _log.Content("事件内容：").Content(state.SafeString());
            }
            if (!(state is IEnumerable list))
            {
                return;
            }
            var dictionary=new Dictionary<string,string>();
            foreach (KeyValuePair<string,object> item in list)
            {
                dictionary.Add(item.Key,item.Value.SafeString());
            }
            AddDictionary(dictionary);
        }

        /// <summary>
        /// 添加字典内容
        /// </summary>
        /// <param name="dictionary">字典</param>
        private void AddDictionary(IDictionary<string, string> dictionary)
        {
            AddElapsed(GetValue(dictionary,"elapsed"));
            var sqlParams = GetValue(dictionary, "parameters");
            AddSql(GetValue(dictionary, "commandText"), sqlParams);
            AddSqlParams(sqlParams);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="dictionary">字典</param>
        /// <param name="key">键</param>
        /// <returns></returns>
        private string GetValue(IDictionary<string, string> dictionary, string key)
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }
            return string.Empty;
        }

        /// <summary>
        /// 添加执行时间
        /// </summary>
        /// <param name="value">执行时间</param>
        private void AddElapsed(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }
            _log.Content($"执行时间：{value} 毫秒");
        }

        /// <summary>
        /// 添加Sql
        /// </summary>
        /// <param name="sql">Sql语句</param>
        /// <param name="sqlParams">Sql参数</param>
        private void AddSql(string sql, string sqlParams)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                return;
            }
            _log.Sql("原始Sql：").Sql($"{sql}{Common.Line}");
            sql = sql.Replace("SET NOCOUNT ON;", "");
            _log.Sql($"调试Sql：{GetSql(sql, sqlParams)}{Common.Line}");
        }

        /// <summary>
        /// 添加Sql参数
        /// </summary>
        /// <param name="value">参数</param>
        private void AddSqlParams(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }
            _log.SqlParams(value);
        }

        /// <summary>
        /// 是否启用
        /// </summary>
        /// <param name="logLevel">日志级别</param>
        /// <returns></returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        /// <summary>
        /// 起始范围
        /// </summary>
        /// <typeparam name="TState">状态类型</typeparam>
        /// <param name="state">状态</param>
        /// <returns></returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        /// <summary>
        /// 获取Sql
        /// </summary>
        /// <param name="sql">Sql语句</param>
        /// <param name="sqlParams">Sql参数</param>
        /// <returns></returns>
        public static string GetSql(string sql, string sqlParams)
        {
            var parameters = GetSqlParameters(sqlParams);
            foreach (var parameter in parameters)
            {
                var regex = new Regex($@"{parameter.Key}\b", RegexOptions.Compiled);
                sql = regex.Replace(sql, parameter.Value);
            }
            return sql;
        }

        /// <summary>
        /// 获取Sql参数字典
        /// </summary>
        /// <param name="sqlParams">Sql参数</param>
        /// <returns></returns>
        public static IDictionary<string, string> GetSqlParameters(string sqlParams)
        {
            var result=new Dictionary<string,string>();
            if (sqlParams == null)
            {
                return result;
            }
            var parameters = sqlParams.Split(',');
            foreach (var parameter in parameters)
            {
                AddSqlParameter(result,parameter);
            }
            return result;
        }

        /// <summary>
        /// 添加Sql参数
        /// </summary>
        /// <param name="result">参数字典</param>
        /// <param name="parameter">参数名</param>
        private static void AddSqlParameter(Dictionary<string, string> result, string parameter)
        {
            var items = parameter.Split('=');
            if (items.Length < 2)
            {
                return;
            }
            result.Add(items[0].Trim(), GetValue(parameter, items[1]));
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="parameter">参数名</param>
        /// <param name="value">参数值</param>
        /// <returns></returns>
        private static string GetValue(string parameter, string value)
        {
            value = value.Substring(0, value.IndexOf("'", 1, StringComparison.Ordinal) + 1).Trim();
            if (value == "''" && parameter.Contains("DbType = Guid"))
            {
                return "null";
            }
            return value;
        }
    }
}
