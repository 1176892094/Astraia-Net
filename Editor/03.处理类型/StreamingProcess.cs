// *********************************************************************************
// # Project: Test
// # Unity: 2022.3.5f1c1
// # Author: jinyijie
// # Version: 1.0.0
// # History: 2024-06-06  05:06
// # Copyright: 2024, jinyijie
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System.Linq;
using System.Runtime.CompilerServices;
using JFramework.Net;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace JFramework.Editor
{
    internal static class StreamingProcess
    {
          /// <summary>
        /// 初始化读写流
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="models"></param>
        /// <param name="writers"></param>
        /// <param name="readers"></param>
        /// <param name="td"></param>
        public static void RuntimeInitializeOnLoad(AssemblyDefinition assembly, Models models, Writer writers, Reader readers, TypeDefinition td)
        {
            var method = new MethodDefinition(nameof(RuntimeInitializeOnLoad), MethodAttributes.Public | MethodAttributes.Static, models.Import(typeof(void)));

            AddRuntimeInitializeOnLoadAttribute(assembly, models, method);

            if (Helper.IsEditorAssembly(assembly))
            {
                AddInitializeOnLoadAttribute(assembly, models, method);
            }

            var worker = method.Body.GetILProcessor();
            writers.InitializeWriters(worker);
            readers.InitializeReaders(worker);
            worker.Emit(OpCodes.Ret);
            td.Methods.Add(method);
        }

        /// <summary>
        /// 添加 RuntimeInitializeLoad 属性类型
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="models"></param>
        /// <param name="md"></param>
        private static void AddRuntimeInitializeOnLoadAttribute(AssemblyDefinition assembly, Models models, MethodDefinition md)
        {
            var ctor = models.RuntimeInitializeOnLoadMethodAttribute.GetConstructors().Last();
            var attribute = new CustomAttribute(assembly.MainModule.ImportReference(ctor));
            attribute.ConstructorArguments.Add(new CustomAttributeArgument(models.Import<RuntimeInitializeLoadType>(), RuntimeInitializeLoadType.BeforeSceneLoad));
            md.CustomAttributes.Add(attribute);
        }

        /// <summary>
        /// 添加 RuntimeInitializeLoad 属性标记
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="models"></param>
        /// <param name="md"></param>
        private static void AddInitializeOnLoadAttribute(AssemblyDefinition assembly, Models models, MethodDefinition md)
        {
            var ctor = models.InitializeOnLoadMethodAttribute.GetConstructors().First();
            var attribute = new CustomAttribute(assembly.MainModule.ImportReference(ctor));
            md.CustomAttributes.Add(attribute);
        }
        
        /// <summary>
        /// 在Process中调用
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="resolver"></param>
        /// <param name="logger"></param>
        /// <param name="writer"></param>
        /// <param name="reader"></param>
        /// <param name="failed"></param>
        /// <returns></returns>
        public static bool Process(AssemblyDefinition assembly, IAssemblyResolver resolver, Logger logger, Writer writer, Reader reader, ref bool failed)
        {
            ProcessAssembly(assembly, resolver, logger, writer, reader, ref failed);
            return ProcessCustomCode(assembly, assembly, writer, reader, ref failed);
        }

        /// <summary>
        /// 处理网络代码
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="resolver"></param>
        /// <param name="logger"></param>
        /// <param name="writer"></param>
        /// <param name="reader"></param>
        /// <param name="failed"></param>
        private static void ProcessAssembly(AssemblyDefinition assembly, IAssemblyResolver resolver, Logger logger, Writer writer, Reader reader, ref bool failed)
        {
            var assemblyRef = assembly.MainModule.AssemblyReferences.FirstOrDefault(reference => reference.Name == Const.ASSEMBLY_NAME);
            if (assemblyRef != null)
            {
                var assemblyDef = resolver.Resolve(assemblyRef);
                if (assemblyDef != null)
                {
                    ProcessCustomCode(assembly, assemblyDef, writer, reader, ref failed);
                }
                else
                {
                    logger.Error($"自动生成网络代码失败: {assemblyRef}");
                }
            }
            else
            {
                logger.Error("注册程序集 JFramework.Net.dll 失败");
            }
        }

        /// <summary>
        /// 处理本地代码
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="assemblyDef"></param>
        /// <param name="writer"></param>
        /// <param name="reader"></param>
        /// <param name="failed"></param>
        /// <returns></returns>
        private static bool ProcessCustomCode(AssemblyDefinition assembly, AssemblyDefinition assemblyDef, Writer writer, Reader reader, ref bool failed)
        {
            var changed = false;
            foreach (var td in assemblyDef.MainModule.Types.Where(td => td.IsAbstract && td.IsSealed))
            {
                changed |= ProcessWriter(assembly, td, writer);
                changed |= ProcessReader(assembly, td, reader);
            }

            foreach (var td in assemblyDef.MainModule.Types)
            {
                changed |= ProcessMessage(assembly.MainModule, writer, reader, td, ref failed);
            }

            return changed;
        }

        /// <summary>
        /// 加载声明的写入器
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="td"></param>
        /// <param name="writer"></param>
        /// <returns></returns>
        private static bool ProcessWriter(AssemblyDefinition assembly, TypeDefinition td, Writer writer)
        {
            var change = false;
            foreach (var md in td.Methods)
            {
                if (md.Parameters.Count != 2)
                    continue;

                if (!md.Parameters[0].ParameterType.Is<MemoryWriter>())
                    continue;

                if (!md.ReturnType.Is(typeof(void)))
                    continue;

                if (!md.HasCustomAttribute<ExtensionAttribute>())
                    continue;

                if (md.HasGenericParameters)
                    continue;

                writer.Register(md.Parameters[1].ParameterType, assembly.MainModule.ImportReference(md));
                change = true;
            }

            return change;
        }

        /// <summary>
        /// 加载声明的读取器
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="td"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static bool ProcessReader(AssemblyDefinition assembly, TypeDefinition td, Reader reader)
        {
            var change = false;
            foreach (var md in td.Methods)
            {
                if (md.Parameters.Count != 1)
                    continue;

                if (!md.Parameters[0].ParameterType.Is<MemoryReader>())
                    continue;

                if (md.ReturnType.Is(typeof(void)))
                    continue;

                if (!md.HasCustomAttribute<ExtensionAttribute>())
                    continue;

                if (md.HasGenericParameters)
                    continue;

                reader.Register(md.ReturnType, assembly.MainModule.ImportReference(md));
                change = true;
            }

            return change;
        }

        /// <summary>
        /// 加载读写流的信息
        /// </summary>
        /// <param name="module"></param>
        /// <param name="writer"></param>
        /// <param name="reader"></param>
        /// <param name="td"></param>
        /// <param name="failed"></param>
        /// <returns></returns>
        private static bool ProcessMessage(ModuleDefinition module, Writer writer, Reader reader, TypeDefinition td, ref bool failed)
        {
            var change = false;
            if (!td.IsAbstract && !td.IsInterface && td.ImplementsInterface<IMessage>())
            {
                reader.GetFunction(module.ImportReference(td), ref failed);
                writer.GetFunction(module.ImportReference(td), ref failed);
                change = true;
            }

            foreach (var nested in td.NestedTypes)
            {
                change |= ProcessMessage(module, writer, reader, nested, ref failed);
            }

            return change;
        }
    }
}