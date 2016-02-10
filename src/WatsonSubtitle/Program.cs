using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace WatsonSubtitle {
    public class Program {
        const string FilePath = "C:\\Data\\onepiece_1.json";
        const string OutFilePath = "C:\\Data\\onepiece_1_clean.json";
        const string OutSRTFile = "C:\\Data\\onepiece_1_subs.srt";
        const string S = "現在のコード ページ";

        public void Main(string[] args) {


            Debug.WriteLine("Processing...");
            //ProcessFile();
            ReadJson();
            Debug.WriteLine("Done");
        }

        void ReadJson() {
            string text = File.ReadAllText(OutFilePath, Encoding.UTF8);
            JsonTextReader reader = new JsonTextReader(new StringReader(text));
            //int initialDepth = -1;

            var currentPhrase = new Phrase();
            var phrases = new List<Phrase>();
            var confidence = new Dictionary<string, double>();

            while (reader.Read()) {
                if (reader.Value != null) {
                    //Debug.WriteLine("Token: {0}, Key: {1}, Value: {2}", reader.TokenType, reader.Path, reader.Value);
                    if (IsProperty(reader, "final")) {
                        reader.Read();
                        if (reader.Value.ToString() == "True" && currentPhrase.Words.Count > 0) {
                            phrases.Add(currentPhrase);
                        }
                        currentPhrase = new Phrase();
                    }

                    if (IsProperty(reader, "timestamps")) {
                        ReadTimestamps(reader, currentPhrase, confidence);
                    }

                    if (IsProperty(reader, "word_confidence")) {
                        ReadConfidence(reader, confidence);
                    }
                } else {
                    //Debug.WriteLine("Token: {0}", reader.TokenType);
                }
            }

            Debug.WriteLine(phrases.Count + " phrases");
            WriteToSRT(phrases);
        }

        bool IsProperty(JsonTextReader reader, string name) {
            return reader.TokenType == JsonToken.PropertyName && reader.Value.ToString() == name;
        }

        void ReadTimestamps(JsonTextReader reader, Phrase currentPhrase, Dictionary<string, double> confidence) {
            reader.Read();
            reader.Read();
            if (reader.TokenType != JsonToken.EndArray) {
                while (true) {
                    reader.Read();

                    var wordText = reader.Value.ToString();
                    var start = reader.ReadAsDouble();
                    var end = reader.ReadAsDouble();
                    var word = new Word() { Text = wordText, StartTime = start.Value, EndTime = end.Value, Confidence = GetConfidence(confidence, wordText) };
                    currentPhrase.Words.Add(word);

                    reader.Read();
                    var tok1 = reader.TokenType;
                    reader.Read();
                    var tok2 = reader.TokenType;
                    if (tok1 == JsonToken.EndArray && tok2 == JsonToken.EndArray) {
                        break;
                    }
                }
            }
        }

        double GetConfidence(Dictionary<string, double> confidence, string key) {
            if (confidence.ContainsKey(key)) {
                return confidence[key];
            }
            return 0;
        }

        void ReadConfidence(JsonTextReader reader, Dictionary<string, double> wordConfidences) {
            reader.Read();
            reader.Read();
            if (reader.TokenType != JsonToken.EndArray) {
                while (true) {
                    var wordText = reader.ReadAsString();
                    var conf = reader.ReadAsDouble();
                    wordConfidences[wordText] = conf.Value;

                    reader.Read();
                    var tok1 = reader.TokenType;
                    reader.Read();
                    var tok2 = reader.TokenType;
                    if (tok1 == JsonToken.EndArray && tok2 == JsonToken.EndArray) {
                        break;
                    }
                }
            }
        }

        string ConvertToSRTTimeString(double seconds) {
            var mill = (int)(1000 * (seconds - (int)seconds));
            var s = new TimeSpan(0, 0, 0, (int)seconds, mill).ToString();
            return s.Substring(0, s.Length - 4).Replace(".", ",");
        }

        void WriteToSRT(List<Phrase> phrases) {
            var fileText = new StringBuilder();
            for (int i = 0; i < phrases.Count; i++) {
                fileText.AppendLine((i + 1).ToString());
                fileText.AppendLine(ConvertToSRTTimeString(phrases[i].GetStart()) + " --> " + ConvertToSRTTimeString(phrases[i].GetEnd()));
                fileText.AppendLine(phrases[i].ToString());
                fileText.AppendLine();
            }
            File.WriteAllText(OutSRTFile, fileText.ToString(), Encoding.Unicode);
        }

        void ProcessFile() {
            string text = File.ReadAllText(FilePath, Encoding.UTF8);
            Debug.WriteLine(text.Length + " chars");
            var newText = new StringBuilder();
            newText.Append("[");
            var stack = 0;
            for (int i = 0; i < text.Length; i++) {
                newText.Append(text[i]);

                if (text[i] == '{') {
                    stack++;
                } else if (text[i] == '}') {
                    stack--;
                    if (stack == 0) {
                        newText.Append(',');
                    }
                }
            }
            newText.Append("]");

            File.WriteAllText(OutFilePath, newText.ToString());
        }
    }
}
