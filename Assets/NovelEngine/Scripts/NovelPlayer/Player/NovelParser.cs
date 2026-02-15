using novel;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;

public static class NovelParser
{
    #region 정규표현식 정의
    private static readonly Regex LabelLine = new Regex(@"^#(?<name>.+?)\s*$");
    private static readonly Regex CommentLine = new Regex(@"^\s*//.*$");
    private static readonly Regex CommandLine = new Regex(@"^@\s*");
    private static readonly Regex PersonLine = new Regex(@"(?<name>.[\w가-힣?]+)\s*:\s*(?<line>.+)\s*$");

    #region 커맨드 정규식
    private static readonly Regex BackCommand = new Regex(@"^@back\s+(?<name>\w+)(\.(?<transition>\w+))?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex BGMCommand = new Regex(@"^@bgm\s+(?<name>\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex SfxCommand = new Regex(@"^@sfx\s+(?<name>\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex CharCommand = new Regex(@"^@char\s+(?<name>[\w가-힣?]+)(\.(?<appearance>\w+))?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex GotoCommand = new Regex(@"^@goto\s+(?<script>\w+)?(\.(?<label>\w+))?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ChoiceCommand = new Regex(@"^@choice\s+(?<choice>.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex IfCommand = new Regex(@"^@if\s+(?<var>\w+)\s*(?<op>>=|<=|==|!=|>|<)\s*(?<value>\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ElseCommand = new Regex(@"^@else", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex HideCommand = new Regex(@"^@hide\s+(?<name>[\w가-힣?]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex HideAllCommand = new Regex(@"^@hideall\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex hideCharsCommand = new Regex(@"^@hidechars\s+(?<name>\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex hidePrinterCommand = new Regex(@"^@hideprinter\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
   
    private static Regex hideUICommand = new Regex(@"^@hideui\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex showUICommand = new Regex(@"^@showui\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    
    private static Regex stopCommand = new Regex(@"^@stop\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex StopBGMCommand = new Regex(@"^@stopbgm\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex stopSfxCommand = new Regex(@"^@stopsfx\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex stopVoiceCommand = new Regex(@"^@stopvoice\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static Regex setCommand = new Regex(@"^@set\s*(?<left>\w+)\s*(?<op>=|\+\+|\-\-|\+=|\-=)\s*(?<right>("".*?"")|\w+)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static readonly Regex WaitCommand = new Regex(@"^@wait(?:\s+(?<time>[\d.]+))?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    #endregion

    #region 커맨드 매개변수 정규식
    private static readonly Regex PosPattern = new Regex(@"pos\s*:\s*(?<posX>[\d.]+)\s*,\s*(?<posY>[\d.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ScalePattern = new Regex(@"scale\s*:\s*(?<scale>[\d.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex TimePattern = new Regex(@"time\s*:\s*(?<time>[\d.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex IfPattern = new Regex(@"if\s*:\s*(?<var>\w+)\s*(?<op>>=|<=|==|!=|>|<)\s*(?<value>[\d.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex VolumePattern = new Regex(@"volume\s*:\s*(?<volume>[\d.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex LoopPattern = new Regex(@"(?<loop>!loop|loop!)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static  Regex fadePattern = new Regex(@"fade\s*:\s*(?<fade>[\d.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex TransitionPattern = new Regex(@"transition\s*:\s*(?<transition>\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex SubLinePattern = new Regex("^ {4}(?<argument>.*)", RegexOptions.Compiled);

    private static readonly Regex FlipPattern = new Regex(@"flip\s*:\s*(?<isFlip>true|False)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    // set 관련 매개변수
    #endregion
    #endregion
    public static NovelAct Parse(string[] lines)
    {

        NovelAct act = new() { novelLines = new List<NovelLine>() };
        List<NovelLine> novelLines = new();
        int index = 1;

        foreach(string raw in lines)
        {
            NovelLine line = ParseLine(act, raw, index, novelLines);
            if (line != null)
            {
                novelLines.Add(line);

                index++;
            }

        }
        act.novelLines = novelLines;
        Debug.Log("파싱 끝");
        return act; 
    }
    private static DialogoueType LineType(string line)
    {
        if (CommentLine.IsMatch(line))
            return DialogoueType.CommentLine;
        else if (LabelLine.IsMatch(line))
            return DialogoueType.LabelLine;
        else if (CommandLine.IsMatch(line))
            return DialogoueType.CommandLine;
        else if (PersonLine.IsMatch(line))
            return DialogoueType.PersonLine;
        else
            return DialogoueType.NormalLine;
    }
    private static NovelLine ParseLine(NovelAct act, string line, int index, List<NovelLine> lineList = null)
    {
        NovelLine result = null;
        if (SubLinePattern.IsMatch(line))
        {
            // 공백4개 제거
            line = line.Substring(4);

            if (lineList[index - 2] is CommandLine command)
            {
                var subLine = ParseLine(act, line, index - 1);
                if (subLine != null)
                {
                    command.subLine = subLine;
                    //Debug.Log($"Subline of {command.GetType().Name} at index {index - 1} parsed.");
                }
            }
            // 서브라인 파싱은 직접 넣어주고 아무것도 리턴하지 않음
            return null;
        }
        else
        {
            DialogoueType type = LineType(line);
            switch (type)
            {
                case DialogoueType.CommandLine:
                    result = ParseCommand(index, line);
                    break;
                case DialogoueType.NormalLine:
                    string normalLine = line.Trim();
                    if (normalLine == null || normalLine == "")
                    {
                        return null;
                    }
                    //Debug.Log($"Normal : {line}\nIndex : {index}");

                    result = new NormalLine(index, normalLine);
                    break;
                case DialogoueType.PersonLine:
                    var personMatch = PersonLine.Match(line);
                    string actorName = personMatch.Groups["name"].Value;
                    string actorLine = personMatch.Groups["line"].Value;

                    //Debug.Log($"Person : {actorName}, Line : {actorLine}\nIndex : {index}");
                    result = new PersonLine(index, actorName, actorLine);
                    break;
                case DialogoueType.LabelLine:
                    var labelMatch = LabelLine.Match(line);
                    string labelName = labelMatch.Groups["name"].Value;

                    NovelManager.Player.LabelDict.Add(labelName, index);
                    //Debug.Log($"Label Name : {labelName}\nIndex : {index}");
                    result = new LabelLine(index, labelName);
                    break;
            }
        }
        if (result == null)
        {
            Debug.LogWarning($"파싱 실패 : {line} at {index} line" );
            return null;
        }
        else
            return result;
    }
    private static CommandType GetCommandType(string line)
    {
        if (CharCommand.IsMatch(line))
            return CommandType.ShowCharacter;
        else if (HideCommand.IsMatch(line))
            return CommandType.HideCharacter;
        else if (HideAllCommand.IsMatch(line))
            return CommandType.HideAll;
        else if (hideUICommand.IsMatch(line))
            return CommandType.HideUI;
        else if (showUICommand.IsMatch(line))
            return CommandType.ShowUI;
        else if (ChoiceCommand.IsMatch(line))
            return CommandType.Choice;
        else if (GotoCommand.IsMatch(line))
            return CommandType.Goto;
        
        else if (IfCommand.IsMatch(line))
            return CommandType.If;
        else if (ElseCommand.IsMatch(line))
            return CommandType.Else;
        
        else if (setCommand.IsMatch(line))
            return CommandType.Set;
        
        else if (WaitCommand.IsMatch(line))
            return CommandType.Wait;

        else if (BackCommand.IsMatch(line))
            return CommandType.Background;

        else if (BGMCommand.IsMatch(line))
            return CommandType.BGM;
        else if (StopBGMCommand.IsMatch(line))
            return CommandType.StopBGM;
        else if (SfxCommand.IsMatch(line))
            return CommandType.Sfx;
        else
            return CommandType.None;
    }
    private static CompOp ParseCompOp(string opStr)
    {
        return opStr switch
        {
            ">" => CompOp.GreaterThan,
            "<" => CompOp.LessThan,
            ">=" => CompOp.GreaterThanOrEqual,
            "<=" => CompOp.LessThanOrEqual,
            "==" => CompOp.Equal,
            "!=" => CompOp.NotEqual,
            _ => throw new ArgumentException($"Invalid operator: {opStr}")
        };
    }

    private static CalcOp ParseCalcOp(string opStr)
    {
        return opStr switch
        {
            "=" => CalcOp.Assign,
            "++" => CalcOp.Increase,
            "--" => CalcOp.Decrease,
            "+=" => CalcOp.IncreaseAmount,
            "-=" => CalcOp.DecreaseAmount,
            _ => throw new ArgumentException($"Invalid operator: {opStr}")
        };
    }
    private static IfParameter ParseIfParameter(string line)
    {
        var ifMatch = IfPattern.Match(line);
        IfParameter ifParameter = new();
        if (ifMatch.Success)
        {
            ifParameter = new IfParameter(
                ifMatch.Groups["var"].Value,
                ParseCompOp(ifMatch.Groups["op"].Value),
                ifMatch.Groups["value"].Value);
        }
        return ifParameter;
    }
    private static NovelLine ParseCommand(int index, string line)
    {
        CommandType commandType = GetCommandType(line);
        NovelLine result = null;

        switch (commandType)
        {
            case CommandType.None:
                Debug.LogWarning($"Unknown command at line {index}: {line}");
                break;
            case CommandType.ShowCharacter:
                {
                    var charMatch = CharCommand.Match(line);
                    string charName = charMatch.Groups["name"].Value;
                    string charAppearance = charMatch.Groups["appearance"].Value.ToLower();

                    Vector2? charPos = null;
                    var charPosMatch = PosPattern.Match(line);
                    if (charPosMatch.Success)
                    {
                        float x = float.Parse(charPosMatch.Groups["posX"].Value);
                        float y = float.Parse(charPosMatch.Groups["posY"].Value);
                        charPos = new Vector2(x, y);
                    }

                    float? charScale = null;
                    var charScaleMatch = ScalePattern.Match(line);
                    if (charScaleMatch.Success)
                        charScale = float.Parse(charScaleMatch.Groups["scale"].Value);

                    string charTransition = null;
                    var charTransitionMatch = TransitionPattern.Match(line);
                    if (charTransitionMatch.Success)
                        charTransition = charTransitionMatch.Groups["transition"].Value.ToLower();

                    float? charTime = null;
                    var charTimeMatch = TimePattern.Match(line);
                    if (charTimeMatch.Success)
                        charTime = float.Parse(charTimeMatch.Groups["time"].Value);

                    bool isFlip = false;
                    var isFlipMatch = FlipPattern.Match(line);
                    if (isFlipMatch.Success)
                    {
                        isFlip = isFlipMatch.Groups["isFlip"].Value == "true";
                    }
                    
                    
                    
                    CharCommandType charCommandType = CharCommandType.Show;
                    if (charTransition == "fadeout")
                    {
                        charCommandType = CharCommandType.Hide;
                    }

                    IfParameter ifParameter = ParseIfParameter(line);

                    result = new CharCommand(index, charName, charAppearance, charTransition, charPos, charScale, charTime, isFlip, charCommandType, ifParameter);
                    // Debug.Log(
                    //     $"Character : {charName}\n" +
                    //     $"Pos : {charPos}\n" +
                    //     $"Appearance : {charAppearance}\n" +
                    //     $"Scale : {charScale}\n" +
                    //     $"transition : {charTransition}\n" +
                    //     $"Type : {charCommandType}\n" +
                    //     $"time : {charTime}\n" +
                    //     $"flip : {isFlip}\n" +
                    //     $"Index : {index}\n"+
                    //     $"If : {ifParameter.Var} {ifParameter.Op} {ifParameter.Value}");
                }
                break;
            case CommandType.HideCharacter:
                {
                    var hideMatch = HideCommand.Match(line);
                    string name = hideMatch.Groups["name"].Value;

                    IfParameter ifParameter = ParseIfParameter(line);

                    result = new CharCommand(index, name, null, null, null, null, null, false, CharCommandType.Hide, ifParameter);
                    // Debug.Log(
                    //     $"hide command\nCharacter : {name}\n" +
                    //     $"Index : {index}\n" +
                    //     $"If : {ifParameter.Var} {ifParameter.Op} {ifParameter.Value}");
                }
                break;
            case CommandType.HideAll:
                {
                    IfParameter ifParameter = ParseIfParameter(line);

                    result = new CharCommand(index, null, null, null, null, null, null, false, CharCommandType.HideAll, ifParameter);
                    // Debug.Log($"Hide All Command\n" +
                    //     $"Index : {index}\n" +
                    //     $"If : {ifParameter.Var} {ifParameter.Op} {ifParameter.Value}");
                }
                break;
            case CommandType.Clearall:
                break;
            case CommandType.Choice:
                {
                    string choice = null;
                    var choiceMatch = ChoiceCommand.Match(line);
                    if (choiceMatch.Success)
                        choice = choiceMatch.Groups["choice"].Value;

                    var ifMatch = IfPattern.Match(line);
                    if (ifMatch.Success)
                    {
                        var ifString = ifMatch.Value;
                        choice = choice.Replace(ifString, "").Trim();
                    }


                    IfParameter ifParameter = ParseIfParameter(line);

                    result = new ChoiceCommand(index, choice, ifParameter);


                    Debug.Log(
                        $"Choice : {choice}\n" +
                        $"Index : {index}\n" +
                        $"If : {ifParameter.Var} {ifParameter.Op} {ifParameter.Value}");
                }
                break;
            case CommandType.Goto:
                {
                    var gotoMatch = GotoCommand.Match(line);
                    string script = gotoMatch.Groups["script"].Value;
                    string label = gotoMatch.Groups["label"].Value;

                    IfParameter ifParameter = ParseIfParameter(line);

                    result = new GotoCommand(index, script,label, ifParameter);
                    Debug.Log(
                        $"Goto : {label}\n" +
                        $"Index : {index}\n" +
                        $"Script : {script}\n" +
                        $"Label : {label}\n" + 
                        $"If : {ifParameter.Var} {ifParameter.Op} {ifParameter.Value}");
                }
                break;
            case CommandType.If:
                {
                    var ifMatch = IfCommand.Match(line);
                    string var = ifMatch.Groups["var"].Value;
                    CompOp op = ParseCompOp(ifMatch.Groups["op"].Value);
                    string value = ifMatch.Groups["value"].Value;

                    IfParameter ifParameter = ParseIfParameter(line);

                    result = new IfCommand(index, IfType.If, var, op, value);
                    // Debug.Log($"If Command : {var} {op} {value}");
                }
                break;
            case CommandType.Else:
                {
                    IfParameter ifParameter = ParseIfParameter(line);
                    result = new IfCommand(index, IfType.Else);
                    // Debug.Log($"Else Command :");
                }
                break;
            case CommandType.Set:
                {
                    var setMatch = setCommand.Match(line);
                    string left = setMatch.Groups["left"].Value;
                    CalcOp op = ParseCalcOp(setMatch.Groups["op"].Value);
                    string right = setMatch.Groups["right"].Value;
                    
                    NovelVariable rightVariable = null;
                    NovelVariable leftVariable = null;
                    
                    
                    
                    
                    Debug.Log($"Set : {left} {op} {right}");
                }
                break;
            case CommandType.Wait:
                {
                    var waitMatch = WaitCommand.Match(line);

                    float? waitTime = waitMatch.Groups["time"].Success ? float.Parse(waitMatch.Groups["time"].Value) : null;
                    
                    IfParameter ifParameter = ParseIfParameter(line);

                    result = new WaitCommand(index, waitTime, ifParameter);
                    // Debug.Log(
                    //     $"Wait \n" +
                    //     $"Time : {waitTime}\n" +
                    //     $"Index : {index}" +
                    //     $"If : {ifParameter.Var} {ifParameter.Op} {ifParameter.Value}");
                }
                break;
            case CommandType.Background:
                {
                    var match = BackCommand.Match(line);

                    string backName = match.Groups["name"].Value;
                    string backgroundTransition = match.Groups["transition"].Value;

                    Vector2? backgroundPos = null;
                    var backgroundPosMatch = PosPattern.Match(line);
                    if (backgroundPosMatch.Success)
                    {
                        float x = float.Parse(backgroundPosMatch.Groups["posX"].Value);
                        float y = float.Parse(backgroundPosMatch.Groups["posY"].Value);
                        backgroundPos = new Vector2(x, y);
                    }

                    float? backgroundScale = null;
                    var backgroundScaleMatch = ScalePattern.Match(line);
                    if (backgroundScaleMatch.Success)
                        backgroundScale = float.Parse(backgroundScaleMatch.Groups["scale"].Value);

                    float? backgroundTime = null;
                    var backgroundTimeMatch = TimePattern.Match(line);
                    if (backgroundTimeMatch.Success)
                        backgroundTime = float.Parse(backgroundTimeMatch.Groups["time"].Value);

                    IfParameter ifParameter = ParseIfParameter(line);

                    result = new BackCommand(
                        index, 
                        backName, 
                        backgroundTransition, 
                        backgroundPos,
                        backgroundScale, 
                        backgroundTime,
                        ifParameter);


                    // Debug.Log(
                    //     $"BackName : {backName}\n" +
                    //     $"Transition : {backgroundTransition}\n" +
                    //     $"pos : {backgroundPos}\n" +
                    //     $"scale : {backgroundScale}\n" +
                    //     $"Time : {backgroundTime}\n" +
                    //     $"Index : {index}" +
                    //     $"If : {ifParameter.Var} {ifParameter.Op} {ifParameter.Value}");

                }

                break;
            case CommandType.BGM:
                {
                    var bgmMatch = BGMCommand.Match(line);
                    string bgmName = bgmMatch.Groups["name"].Value;

                    int? volume = null;
                    var volumeMatch = VolumePattern.Match(line);
                    if (volumeMatch.Success)
                        volume = int.Parse(volumeMatch.Groups["volume"].Value);

                    bool loop = false;
                    var loopMatch = LoopPattern.Match(line);
                    if (loopMatch.Success)
                    {
                        var loopStr = loopMatch.Groups["loop"].Value;
                        if (loopStr == "!loop")
                            loop = true;
                    }


                    IfParameter ifParameter = ParseIfParameter(line);

                    result = new SoundCommand(index, bgmName, volume, loop, true, NovelSound.Bgm ,ifParameter);
                    // Debug.Log(
                    //     $"BGM : {bgmName}\n" +
                    //     $"volume : {volume}\n" +
                    //     $"loop : {loop}\n" +
                    //     $"Index : {index}\n" +
                    //     $"If : {ifParameter.Var} {ifParameter.Op} {ifParameter.Value}");

                }

                break;
            case CommandType.StopBGM:

                {
                    IfParameter ifParameter = ParseIfParameter(line);
                    result = new SoundCommand(index, null, null, false, false, NovelSound.Bgm,ifParameter);
                    // Debug.Log(
                    //     $"StopBGM\n" +
                    //     $"Index : {index}" +
                    //     $"If : {ifParameter.Var} {ifParameter.Op} {ifParameter.Value}");
                }

                break;
            case CommandType.Sfx:
                {
                    var sfxMatch = BGMCommand.Match(line);
                    string sfxName = sfxMatch.Groups["name"].Value;

                    int? volume = null;
                    var volumeMatch = VolumePattern.Match(line);
                    if (volumeMatch.Success)
                        volume = int.Parse(volumeMatch.Groups["volume"].Value);


                    bool loop = false;
                    var loopMatch = LoopPattern.Match(line);
                    if (loopMatch.Success)
                    {
                        var loopStr = loopMatch.Groups["loop"].Value;
                        if (loopStr == "!loop")
                            loop = true;
                    }

                    IfParameter ifParameter = ParseIfParameter(line);
                    result = new SoundCommand(index, sfxName, volume, loop, true, NovelSound.Effect, ifParameter);
                    // Debug.Log(
                    //     $"SFX : {sfxName}\n" +
                    //     $"volume : {volume}\n" +
                    //     $"loop : {loop}\n" +
                    //     $"Index : {index}" +
                    //     $"If : {ifParameter.Var} {ifParameter.Op} {ifParameter.Value}");
                }
                break;
            case CommandType.StopSfx:
                {
                    IfParameter ifParameter = ParseIfParameter(line);
                    result = new SoundCommand(index, null, null, false, false, NovelSound.Effect, ifParameter);
                    // Debug.Log(
                    //     $"StopSFX\n" +
                    //     $"Index : {index}" +
                    //     $"If : {ifParameter.Var} {ifParameter.Op} {ifParameter.Value}");
                }
                break;
            case CommandType.Effect:
            case CommandType.HideUI:
                {
                    result = new HideUICommand(index, false);
                }
                break;
            case CommandType.ShowUI:
                {
                    result = new HideUICommand(index, true);
                }
                break;
        }

        if (result == null)
        {
            return null;
        }
        else
            return result;

    }
    public class IfParameter
    {
        public readonly string Var;
        public readonly CompOp Op;
        public readonly string Value;
        public IfParameter()
        {
            this.Var = null;
            this.Op = CompOp.None;
            this.Value = null;
        }
        public IfParameter(string var, CompOp op, string value)
        {
            this.Var = var;
            this.Op = op;
            this.Value = value;
        }

    }
}