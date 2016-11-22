﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QuantConnect.Brokerages.InteractiveBrokers
{
    /// <summary>
    /// Provides the mapping between Lean symbols and InteractiveBrokers symbols.
    /// </summary>
    public class InteractiveBrokersSymbolMapper : ISymbolMapper
    {
        // we have a special treatment of futures, because IB renamed several exchange tickers (like GBP instead of 6B). We fix this: 
        // We map those tickers back to their original names using the map below
        private Dictionary<string, string> _ibNameMap = new Dictionary<string, string>();

        /// <summary>
        /// Constructs InteractiveBrokersSymbolMapper
        /// </summary>
        public InteractiveBrokersSymbolMapper()
        {
            var ibNameMapFileName = "IB-symbol-map.json";
            var ibNameMapFullName = Path.Combine("InteractiveBrokers", ibNameMapFileName);

            if (File.Exists(ibNameMapFullName))
            {
                _ibNameMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(ibNameMapFullName));
            }

        }
        /// <summary>
        /// Converts a Lean symbol instance to an InteractiveBrokers symbol
        /// </summary>
        /// <param name="symbol">A Lean symbol instance</param>
        /// <returns>The InteractiveBrokers symbol</returns>
        public string GetBrokerageSymbol(Symbol symbol)
        {
            if (symbol == null || symbol == Symbol.Empty || string.IsNullOrWhiteSpace(symbol.Value))
                throw new ArgumentException("Invalid symbol: " + (symbol == null ? "null" : symbol.ToString()));

            if (symbol.ID.SecurityType != SecurityType.Forex &&
                symbol.ID.SecurityType != SecurityType.Equity &&
                symbol.ID.SecurityType != SecurityType.Option &&
                symbol.ID.SecurityType != SecurityType.Future)
                throw new ArgumentException("Invalid security type: " + symbol.ID.SecurityType);

            if (symbol.ID.SecurityType == SecurityType.Forex && symbol.Value.Length != 6)
                throw new ArgumentException("Forex symbol length must be equal to 6: " + symbol.Value);

            if (symbol.ID.SecurityType == SecurityType.Option)
            {
                return symbol.Underlying.Value;
            }
            if (symbol.ID.SecurityType == SecurityType.Future)
            {
                return GetBrokerageRootSymbol(symbol.Underlying.Value);
            }

            return symbol.Value;
        }

        /// <summary>
        /// Converts an InteractiveBrokers symbol to a Lean symbol instance
        /// </summary>
        /// <param name="brokerageSymbol">The InteractiveBrokers symbol</param>
        /// <param name="securityType">The security type</param>
        /// <param name="market">The market</param>
        /// <param name="expirationDate">Expiration date of the security(if applicable)</param>
        /// <param name="strike">The strike of the security (if applicable)</param>
        /// <param name="optionRight">The option right of the security (if applicable)</param>
        /// <returns>A new Lean Symbol instance</returns>
        public Symbol GetLeanSymbol(string brokerageSymbol, SecurityType securityType, string market, DateTime expirationDate = default(DateTime), decimal strike = 0, OptionRight optionRight = 0)
        {
            if (string.IsNullOrWhiteSpace(brokerageSymbol))
                throw new ArgumentException("Invalid symbol: " + brokerageSymbol);

            if (securityType != SecurityType.Forex &&
                securityType != SecurityType.Equity &&
                securityType != SecurityType.Option &&
                securityType != SecurityType.Future)
                throw new ArgumentException("Invalid security type: " + securityType);

            if (securityType == SecurityType.Future)
            {
                return Symbol.CreateFuture(GetLeanRootSymbol(brokerageSymbol), market, expirationDate);
            }
            else if (securityType == SecurityType.Option)
            {
                return Symbol.CreateOption(brokerageSymbol, market, OptionStyle.American, optionRight, strike, expirationDate);
            }

            return Symbol.Create(brokerageSymbol, securityType, market);
        }


        /// <summary>
        /// IB specific versions of the symbol mapping (GetBrokerageRootSymbol) for future root symbols
        /// </summary>
        /// <param name="rootSymbol">LEAN root symbol</param>
        /// <returns></returns>
        public string GetBrokerageRootSymbol(string rootSymbol)
        {
            var brokerageSymbol = _ibNameMap.Where(kv => kv.Value == rootSymbol).FirstOrDefault();

            return !string.IsNullOrEmpty(brokerageSymbol.Key) ? brokerageSymbol.Key : rootSymbol;
        }

        /// <summary>
        /// IB specific versions of the symbol mapping (GetLeanRootSymbol) for future root symbols
        /// </summary>
        /// <param name="brokerageRootSymbol">IB Brokerage root symbol</param>
        /// <returns></returns>
        public string GetLeanRootSymbol(string brokerageRootSymbol)
        {
            return _ibNameMap.ContainsKey(brokerageRootSymbol) ? _ibNameMap[brokerageRootSymbol] : brokerageRootSymbol;
        }

    }
}