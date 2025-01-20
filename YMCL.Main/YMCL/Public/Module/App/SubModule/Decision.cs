﻿using YMCL.Public.Enum;

namespace YMCL.Public.Module.App;

public class Decision
{
    public static (bool ifShow, int page) WhetherToShowInitView()
    {
        if (Data.Setting.Language.Code == null)
        {
            return (true, 1);
        }

        if (Data.Setting.WindowTitleBarStyle == Setting.WindowTitleBarStyle.Unset)
        {
            return (true, 2);
        }

        if (!Data.Setting.IsCompleteMinecraftFolderInitialize)
        {
            return (true, 3);
        }

        if (!Data.Setting.IsCompleteJavaInitialize)
        {
            return (true, 4);
        }

        if (!Data.Setting.IsCompleteAccountInitialize)
        {
            return (true, 5);
        } 
        
        return (false, 0);
    }
}