/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: InsiderBuild.cs
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
using System.Globalization;

namespace NinjaCatDiscordBot.Models {
  /// <summary>
  /// Specifies the type of Insider build.
  /// </summary>
  public enum InsiderBuildType {
    Experimental,
    Beta,
    ReleasePreview,
    Server
  }

  /// <summary>
  /// Represents an Insider build.
  /// </summary>
  public class InsiderBuild {
    /// <summary>
    /// Specifies the build type names.
    /// </summary>
    public static readonly Dictionary<InsiderBuildType, string> Names = new() {
      { InsiderBuildType.Experimental, "Experimental" },
      { InsiderBuildType.Beta, "Beta" },
      { InsiderBuildType.ReleasePreview, "Release Preview" },
      { InsiderBuildType.Server, "Server" }
    };

    /// <summary>
    /// Specifies the build type emotes.
    /// </summary>
    public static readonly Dictionary<InsiderBuildType, string> Emotes = new() {
      { InsiderBuildType.Experimental, "test_tube" },
      { InsiderBuildType.Beta, "paintbrush" },
      { InsiderBuildType.ReleasePreview, "package" },
      { InsiderBuildType.Server, "desktop" }
    };

    /// <summary>
    /// Specifies the build type roles.
    /// </summary>
    public static readonly Dictionary<InsiderBuildType, RoleType> Roles = new() {
      { InsiderBuildType.Experimental, RoleType.InsiderExperimental },
      { InsiderBuildType.Beta, RoleType.InsiderBeta },
      { InsiderBuildType.ReleasePreview, RoleType.InsiderReleasePreview },
      { InsiderBuildType.Server, RoleType.None }
    };

    /// <summary>
    /// Gets or sets the type of Insider build.
    /// </summary>
    public InsiderBuildType Type { get; set; }

    /// <summary>
    /// Gets or sets the subtype of Insider build.
    /// </summary>
    public string SubType { get; set; }

    /// <summary>
    /// Gets the subtype display name of Insider build.
    /// </summary>
    public string GetDisplayName() {
      var textInfo = CultureInfo.CurrentCulture.TextInfo;
      return textInfo.ToTitleCase(SubType.Replace('-', ' ').ToLowerInvariant());
    }

    /// <summary>
    /// Gets or sets the build number.
    /// </summary>
    public string BuildNumber { get; set; }

    /// <summary>
    /// Gets or sets the full link to the build release notes.
    /// </summary>
    public string Link { get; set; }
  }
}
