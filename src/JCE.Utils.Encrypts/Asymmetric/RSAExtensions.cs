﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace JCE.Utils.Encrypts.Asymmetric
{
    /// <summary>
    /// RSA 加密扩展
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class RSAExtensions
    {
        /// <summary>
        /// 公钥
        /// </summary>
        private const string PublicKeyFormat =
            @"<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>";

        /// <summary>
        /// 私钥
        /// </summary>
        private const string PrivateKeyFormat =
            @"<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>";

        /// <summary>
        /// 获取RSA Xml序列化
        /// </summary>
        /// <param name="rsa">RSA实例</param>
        /// <param name="includePrivateParameters">是否包含私钥</param>
        /// <returns></returns>
        public static string ToExtXmlString(this RSA rsa, bool includePrivateParameters)
        {
            RSAParameters parameters = rsa.ExportParameters(includePrivateParameters);

            if (includePrivateParameters)
            {
                return string.Format(PrivateKeyFormat,
                    parameters.Modulus != null ? Convert.ToBase64String(parameters.Modulus) : null,
                    parameters.Exponent != null ? Convert.ToBase64String(parameters.Exponent) : null,
                    parameters.P != null ? Convert.ToBase64String(parameters.P) : null,
                    parameters.Q != null ? Convert.ToBase64String(parameters.Q) : null,
                    parameters.DP != null ? Convert.ToBase64String(parameters.DP) : null,
                    parameters.DQ != null ? Convert.ToBase64String(parameters.DQ) : null,
                    parameters.InverseQ != null ? Convert.ToBase64String(parameters.InverseQ) : null,
                    parameters.D != null ? Convert.ToBase64String(parameters.D) : null);
            }
            return string.Format(PublicKeyFormat,
                parameters.Modulus != null ? Convert.ToBase64String(parameters.Modulus) : null,
                parameters.Exponent != null ? Convert.ToBase64String(parameters.Exponent) : null);
        }

        
    }
}