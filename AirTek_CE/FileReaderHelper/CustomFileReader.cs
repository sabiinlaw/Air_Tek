using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace AirTek_CE
{
    public class CustomFileReader : IEnumerable<List<Order>>, IDisposable
    {
        readonly StreamReader _streamReader;
        readonly int _batchSize = 100;

        public CustomFileReader(string path, int batchSize)
        {
            if (batchSize > 0)
            {
                _batchSize = batchSize;
            }
            else
            {
                throw new ArgumentException("Batch size should be greater than Zero", "batchSize");
            }

            _streamReader = File.OpenText(path);
        }

        public void Dispose()
        {
            _streamReader?.Close();
        }

        public IEnumerator<List<Order>> GetEnumerator()
        {
            string interrupString = null;
            while (!_streamReader.EndOfStream)
            {
                int i = 0;
                int j = 0;
                string inputRaw;

                var jsonNodeEnd = "},";
                var jsonNodePattern = "(},)";
                var json = string.Empty;

                List<Order> batch = new List<Order>();
                while (i < _batchSize && !string.IsNullOrEmpty(inputRaw = interrupString ?? _streamReader.ReadLine()))
                {                   
                    var inputArray = Regex.Split(inputRaw, jsonNodePattern);
                    if (inputArray.Length > 0)
                        interrupString = null;

                    for (int k = 0; k < inputArray.Length; k++)
                    {
                        var input = inputArray[k]?.Trim();
                        if (input == "{" || input == "}" || input == "")
                            continue;

                        if (j++ == 0)
                        {
                            json = "{\"name\":" + input.Replace(":", ", data:");
                        }
                        else if (input != jsonNodeEnd)
                        {
                            json += input;
                        }
                        else
                        {
                            json += input.Replace(jsonNodeEnd, "}}");
                            dynamic order = JObject.Parse(json);

                            batch.Add(new Order()
                            {
                                Name = order.name,
                                Destination = order.data.destination
                            });

                            json = string.Empty;
                            j = 0;
                            i++;

                            if (i == _batchSize)
                            {
                                interrupString = inputRaw.Replace(jsonNodeEnd, "");
                            }
                        }
                    }
                }

                if (batch.Count != 0)
                {
                    yield return batch;
                }
            }

            Dispose();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
