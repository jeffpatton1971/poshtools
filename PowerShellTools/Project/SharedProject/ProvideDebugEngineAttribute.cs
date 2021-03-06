﻿/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.IO;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudioTools {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    class ProvideDebugEngineAttribute : RegistrationAttribute {
        private readonly string _id, _name;
        private readonly Type _programProvider, _debugEngine;

        public ProvideDebugEngineAttribute(string name, Type programProvider, Type debugEngine, string id) {
            _name = name;
            _programProvider = programProvider;
            _debugEngine = debugEngine;
            _id = id;
        }

        public override void Register(RegistrationContext context) {
            var engineKey = context.CreateKey("AD7Metrics\\Engine\\" + _id);
            engineKey.SetValue("Name", _name);

            engineKey.SetValue("CLSID", _debugEngine.GUID.ToString("B"));
            engineKey.SetValue("ProgramProvider", _programProvider.GUID.ToString("B"));
            engineKey.SetValue("PortSupplier", "{708C1ECA-FF48-11D2-904F-00C04FA302A1}"); // {708C1ECA-FF48-11D2-904F-00C04FA302A1}

            engineKey.SetValue("Attach", 1);
            engineKey.SetValue("AddressBP", 0);
            engineKey.SetValue("AutoSelectPriority", 6);
            engineKey.SetValue("CallstackBP", 1);
            engineKey.SetValue("ConditionalBP", 1);
            engineKey.SetValue("Exceptions", 1);
            engineKey.SetValue("SetNextStatement", 1);
            engineKey.SetValue("RemoteDebugging", 1);
            engineKey.SetValue("HitCountBP", 0);
            engineKey.SetValue("JustMyCodeStepping", 1);
            //engineKey.SetValue("FunctionBP", 1); // TODO: Implement PythonLanguageInfo.ResolveName

            // provide class / assembly so we can be created remotely from the GAC w/o registering a CLSID 
            engineKey.SetValue("EngineClass", _debugEngine.FullName);
            engineKey.SetValue("EngineAssembly", _debugEngine.Assembly.FullName);

            // load locally so we don't need to create MSVSMon which would need to know how to
            // get at our provider type.  See AD7ProgramProvider.GetProviderProcessData for more info
            engineKey.SetValue("LoadProgramProviderUnderWOW64", 1);
            engineKey.SetValue("AlwaysLoadProgramProviderLocal", 1);
            engineKey.SetValue("LoadUnderWOW64", 1);

            using (var incompatKey = engineKey.CreateSubkey("IncompatibleList")) {
                incompatKey.SetValue("guidCOMPlusNativeEng", "{92EF0900-2251-11D2-B72E-0000F87572EF}");
                incompatKey.SetValue("guidCOMPlusOnlyEng", "{449EC4CC-30D2-4032-9256-EE18EB41B62B}");
                incompatKey.SetValue("guidScriptEng", "{F200A7E7-DEA5-11D0-B854-00A0244A1DE2}");
#if DEV10
                incompatKey.SetValue("guidNativeOnlyEng", "{3B476D35-A401-11D2-AAD4-00C04F990171}");
#endif
            }
            
            using (var autoSelectIncompatKey = engineKey.CreateSubkey("AutoSelectIncompatibleList")) {
                autoSelectIncompatKey.SetValue("guidNativeOnlyEng", "{3B476D35-A401-11D2-AAD4-00C04F990171}");
            }

            var clsidKey = context.CreateKey("CLSID");
            var clsidGuidKey = clsidKey.CreateSubkey(_debugEngine.GUID.ToString("B"));
            clsidGuidKey.SetValue("Assembly", _debugEngine.Assembly.FullName);
            clsidGuidKey.SetValue("Class", _debugEngine.FullName);
            clsidGuidKey.SetValue("InprocServer32", context.InprocServerPath);
            clsidGuidKey.SetValue("CodeBase", Path.Combine(context.ComponentPath, _debugEngine.Module.Name));
            clsidGuidKey.SetValue("ThreadingModel", "Free");

            clsidGuidKey = clsidKey.CreateSubkey(_programProvider.GUID.ToString("B"));
            clsidGuidKey.SetValue("Assembly", _programProvider.Assembly.FullName);
            clsidGuidKey.SetValue("Class", _programProvider.FullName);
            clsidGuidKey.SetValue("InprocServer32", context.InprocServerPath);
            clsidGuidKey.SetValue("CodeBase", Path.Combine(context.ComponentPath, _debugEngine.Module.Name));
            clsidGuidKey.SetValue("ThreadingModel", "Free");

            using (var exceptionAssistantKey = context.CreateKey("ExceptionAssistant\\KnownEngines\\" + _id)) {
                exceptionAssistantKey.SetValue("", _name);
            }
        }

        public override void Unregister(RegistrationContext context) {
        }
    }
}
