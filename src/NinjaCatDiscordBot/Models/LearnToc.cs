/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: LearnToc.cs
* 
* Copyright (c) 2026 John Davis
*
* Permission is hereby granted, free of charge, to any person obtaining a
* copy of this software and associated documentation files (the "Software"),
* to deal in the Software without restriction, including without limitation
* the rights to use, copy, modify, merge, publish, distribute, sublicense,
* and/or sell copies of the Software, and to permit persons to whom the
* Software is furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included
* in all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
* OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
* THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
* FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
* IN THE SOFTWARE.
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NinjaCatDiscordBot.Models {
  /// <summary>
  /// Represents a Learn table of contents item.
  /// </summary>
  public class LearnTocItem {
    [JsonPropertyName("href")]
    public string LinkHref { get; set; }

    [JsonPropertyName("toc_title")]
    public string Title { get; set; }

    [JsonPropertyName("children")]
    public List<LearnTocItem> Children { get; set; }
  }

  /// <summary>
  /// Represents a Learn tables of contents.
  /// </summary>
  public class LearnToc {
    [JsonPropertyName("items")]
    public List<LearnTocItem> Items { get; set; }
  }
}
