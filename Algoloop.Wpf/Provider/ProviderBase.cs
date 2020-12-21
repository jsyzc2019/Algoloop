﻿/*
 * Copyright 2019 Capnode AB
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Algoloop.Model;
using Algoloop.Service;
using Algoloop.Wpf.Common;
using QuantConnect.Logging;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace Algoloop.Provider
{
    abstract public class ProviderBase : IProvider
    {
        private bool _isDisposed;
        private ConfigProcess _process;

        public void Abort()
        {
            _process?.Abort();
        }

        public abstract void Download(MarketModel market, SettingModel settings);

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _process?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _process = null;
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }

        internal bool RunProcess(string cmd, string[] args, IDictionary<string, string> configs)
        {
            bool ok = true;
            _process = new ConfigProcess(
                cmd,
                string.Join(" ", args),
                Directory.GetCurrentDirectory(),
                true,
                (line) => Log.Trace(line),
                (line) =>
                {
                    ok = false;
                    Log.Error(line);
                });

            // Set Environment
            StringDictionary environment = _process.Environment;

            // Set config file
            IDictionary<string, string> config = _process.Config;
            string exeFolder = MainService.GetProgramFolder();
            config["debug-mode"] = "true";
            config["composer-dll-directory"] = exeFolder;
            config["results-destination-folder"] = ".";
            config["plugin-directory"] = ".";
            config["log-handler"] = "CompositeLogHandler";
            config["map-file-provider"] = "LocalDiskMapFileProvider";
            config["#command"] = "QuantConnect.ToolBox.exe";
            config["#parameters"] = string.Join(" ", args);
            config["#work-directory"] = Directory.GetCurrentDirectory();
            foreach (KeyValuePair<string, string> item in configs)
            {
                config.Add(item);
            }

            // Start process
            _process.Start();
            if (!_process.WaitForExit())
            {
                ok = false;
            }

            _process.Dispose();
            _process = null;
            return ok;
        }
    }
}
