using System;
using System.Collections.Generic;
using System.Linq;
using Core.Model;

namespace UI.Model;

public class FileSystemMember {
    public string Name { get; }
    public float Percentage { get; }
    public long Size { get; }
    public string Color { get; }
    public string Icon { get; }
    public IList<FileSystemMember> Childs { get; }

    private readonly string[] _palette = {
        "#FF00B023", "#FF4DB21F", "#FF9AB41B", "#FFC0B617", "#FFE5B712",
        "#FFE4920E", "#FFE36E0B", "#FFE24907", "#FFE12504", "#FFE00000"
    };

    public FileSystemMember(FileSystemObject fileSystemObject) {
        Percentage = fileSystemObject.Percentage;
        Size = fileSystemObject.Size;
        Name = fileSystemObject.Name;
        Color = DefineColor(fileSystemObject.Percentage);
        Icon = fileSystemObject.Childs.Count != 0 ? "Folder" : "File";
        Childs = fileSystemObject.Childs.Select(child => new FileSystemMember(child)).ToList();
    }

    private string DefineColor(float percentage) {
        var paletteIndex = (int) Math.Truncate(percentage) / 10 - 1;
        return paletteIndex < 0 ? _palette[paletteIndex + 1] : _palette[paletteIndex];
    }
}