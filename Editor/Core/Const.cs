using Mono.Cecil;

namespace JFramework.Editor
{
    internal struct CONST
    {
        /// <summary>
        /// 程序集名称
        /// </summary>
        public const string ASSEMBLY_NAME = "JFramework.Net";
        
        /// <summary>
        /// 命名空间
        /// </summary>
        public const string GEN_NAMESPACE = "JFramework.Net";
        
        /// <summary>
        /// 生成脚本名称
        /// </summary>
        public const string GEN_NET_CODE = "NetworkGenerator";
        
        /// <summary>
        /// 处理方法名称
        /// </summary>
        public const string GEN_FUNC = "JFrameworkProcessor";
        
        /// <summary>
        /// 调用Rpc取代方法的方法
        /// </summary>
        public const string INV_METHOD = "InvokeRpcMethod";
        
        /// <summary>
        /// Rpc的取代方法
        /// </summary>
        public const string RPC_METHOD = "RpcMethod";
        
        /// <summary>
        /// 序列化网络变量
        /// </summary>
        public const string SER_METHOD = "SerializeSyncVars";
        
        /// <summary>
        /// 反序列化网络变量
        /// </summary>
        public const string DES_METHOD = "DeserializeSyncVars";
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public const string CTOR = ".ctor";

        /// <summary>
        /// 单个网络对象携带的网络变量极限
        /// </summary>
        public const int SYNC_LIMIT = 64;

        /// <summary>
        /// Rpc调用属性
        /// </summary>
        public const MethodAttributes RPC_ATTRS = MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig;
        
        /// <summary>
        /// Reader 和 Writer 属性
        /// </summary>
        public const MethodAttributes RAW_ATTRS = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig;
        
        /// <summary>
        /// 序列化属性
        /// </summary>
        public const MethodAttributes SER_ATTRS = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig;
        
        /// <summary>
        /// 网络变量属性
        /// </summary>
        public const MethodAttributes VAR_ATTRS = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

        /// <summary>
        /// 静态构造属性
        /// </summary>
        public const MethodAttributes CTOR_ATTRS = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig;
        
        /// <summary>
        /// 类型属性
        /// </summary>
        public const TypeAttributes TYPE_ATTRS = TypeAttributes.Class | TypeAttributes.AnsiClass | TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit;
    }
}