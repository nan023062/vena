// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;

namespace Vena.Blockly
{

    [BlocklySource("行为控制/if-else", typeof(BranchNode.Node))]
    public sealed class BranchNode : BehaviorNodeSource
    {
        [ExpressionSignature(typeof(bool))]
        [BlocklySourceProperty("条件", 1)]
        public LogicGraph condition;

        [BlocklySourceProperty("为真时", 2)]
        public BehaviorNodeSource trueSource;

        [BlocklySourceProperty("为假时", 3)]
        public BehaviorNodeSource falseSource;

        sealed class Node : CompositeBehavior<BranchNode>
        {
            LogicGraph.Blockly _condition;

            IBehaviorNode _trueNode;

            IBehaviorNode _falseNode;

            IBehaviorNode _selectedSource;

            protected override void Initialize()
            {
                _condition = blockly.CreateBlockly(source.condition);

                _trueNode = blockly.CreateBlock(source.trueSource);

                _falseNode = blockly.CreateBlock(source.falseSource);
            }

            protected override void OnStart()
            {
                _selectedSource = _trueNode;

                if (_condition != null)
                {
                    var success = _condition.Call<bool>();

                    _selectedSource = success ? _trueNode : _falseNode;
                }
                else
                {
                    LogError("BranchNode: condition is null.");
                }

                _selectedSource?.Start();
            }

            protected override BehaviorResult OnTick(float deltaTime)
            {
                if (null == _selectedSource) return BehaviorResult.Done;

                return _selectedSource.Tick(deltaTime);
            }

            protected override void OnLateTick(float deltaTime)
            {
                _selectedSource?.LateTick(deltaTime);
            }

            protected override void OnFinish()
            {
                _selectedSource?.Finish();
            }

            protected override void OnResetData()
            {
                _selectedSource = null;
            }

            protected override void OnDestroy()
            {
                blockly.DestroyBlockly(_condition);
                blockly.DestroyBlock(_trueNode);
                blockly.DestroyBlock(_falseNode);

                _condition = null;
                _trueNode = null;
                _falseNode = null;
            }
        }
    }

    [BlocklySource("行为控制/switch", typeof(SwitchNode.Node))]
    public sealed class SwitchNode : BehaviorNodeSource
    {
        [ExpressionSignature(typeof(int))]
        [BlocklySourceProperty("分支值", 1)]
        public LogicGraph switchValue;

        [BlocklySourceProperty("分支列表", 2)]
        public BehaviorNodeSource[] caseSources;

        [BlocklySourceProperty("默认分支", 3)]
        public BehaviorNodeSource defaultSource;

        sealed class Node : CompositeBehavior<SwitchNode>
        {
            private LogicGraph.Blockly _caseValue;

            private IBehaviorNode _selectedSource;

            private IBehaviorNode[] _caseNodes;

            private IBehaviorNode _defaultNode;

            protected override void Initialize()
            {
                _selectedSource = null;

                _caseValue = blockly.CreateBlockly(source.switchValue);
                if (_caseValue == null)
                {
                    throw new Exception($"Failed to create _caseValue from source: {source.switchValue}");
                }

                if (source.caseSources != null)
                {
                    _caseNodes = new IBehaviorNode[source.caseSources.Length];

                    for (int i = 0; i < source.caseSources.Length; i++)
                    {
                        var caseSource = source.caseSources[i];

                        var caseNode = blockly.CreateBlock(caseSource);

                        _caseNodes[i] = caseNode ?? throw new Exception($"Failed to create graph node from source: {caseSource}");
                    }
                }

                _defaultNode = blockly.CreateBlock(source.defaultSource);

                if (_defaultNode == null)
                {
                    throw new Exception($"Failed to create graph node from source: {source.defaultSource}");
                }
            }

            protected override void OnStart()
            {
                _selectedSource = _defaultNode;

                if (_caseValue != null)
                {
                    int switchCase = _caseValue.Call<int>();

                    if (_caseNodes != null)
                    {
                        if (switchCase >= 0 && switchCase < _caseNodes.Length)
                        {
                            _selectedSource = _caseNodes[switchCase];
                        }
                    }
                }

                _selectedSource?.Start();
            }

            protected override BehaviorResult OnTick(float deltaTime)
            {
                if (null == _selectedSource) return BehaviorResult.Done;

                return _selectedSource.Tick(deltaTime);
            }

            protected override void OnLateTick(float deltaTime)
            {

            }

            protected override void OnFinish()
            {
                _selectedSource?.Finish();
            }

            protected override void OnResetData()
            {
                _selectedSource = null;
            }

            protected override void OnDestroy()
            {
                blockly.DestroyBlockly(_caseValue);
                _caseValue = null;

                if (_caseNodes != null)
                {
                    foreach (var caseNode in _caseNodes)
                    {
                        blockly.DestroyBlock(caseNode);
                    }
                    _caseNodes = null;
                }
            }
        }
    }

    [BlocklySource("行为控制/选择", typeof(SelectorNode.Node))]
    public sealed class SelectorNode : BehaviorNodeSource
    {
        [BlocklySourceProperty("条件列表", 1)]
        public LogicGraph[] conditions;

        [BlocklySourceProperty("行为列表", 2)]
        public BehaviorNodeSource[] behaviors;

        sealed class Node : CompositeBehavior<SelectorNode>
        {
            LogicGraph.Blockly[] _conditions;

            IBehaviorNode[] _behaviors;

            IBehaviorNode _selected;

            protected override void Initialize()
            {
                _behaviors = null;
                _conditions = null;

                if (source.conditions != null && source.behaviors != null &&
                    source.conditions.Length == source.behaviors.Length)
                {
                    _behaviors = new IBehaviorNode[source.conditions.Length];
                    _conditions = new LogicGraph.Blockly[source.conditions.Length];

                    for (int i = 0; i < source.conditions.Length; i++)
                    {
                        var conditionSource = source.conditions[i];
                        var conditionGraph = blockly.CreateBlockly(conditionSource);
                        if (conditionGraph == null)
                        {
                            throw new Exception($"Failed to create graph node from source: {conditionSource}");
                        }

                        var nodeSource = source.behaviors[i];
                        var node = blockly.CreateBlock(nodeSource);
                        if (node == null)
                        {
                            throw new Exception($"Failed to create graph node from source: {nodeSource}");
                        }
                        _behaviors[i] = node;
                        _conditions[i] = conditionGraph;
                    }
                }
            }

            protected override void OnStart()
            {
                _selected = null;

                if (_conditions != null && _behaviors != null)
                {
                    for (int i = 0; i < _conditions.Length; i++)
                    {
                        var conditionGraph = _conditions[i];

                        var success = conditionGraph.Call<bool>();

                        if (success)
                        {
                            _selected = _behaviors[i];
                            break;
                        }
                    }
                }

                _selected?.Start();
            }

            protected override BehaviorResult OnTick(float deltaTime)
            {
                if (null == _selected) return BehaviorResult.Done;

                return _selected.Tick(deltaTime);
            }

            protected override void OnLateTick(float deltaTime)
            {
            }

            protected override void OnFinish()
            {
                _selected?.Finish();
            }

            protected override void OnResetData()
            {
                _selected = null;
            }

            protected override void OnDestroy()
            {
                foreach (var condition in _conditions)
                {
                    blockly.DestroyBlockly(condition);
                }
                _conditions = null;

                foreach (var behavior in _behaviors)
                {
                    blockly.DestroyBlock(behavior);
                }
                _behaviors = null;
            }
        }
    }

    [BlocklySource("行为控制/循环", typeof(LoopNode.Node))]
    public class LoopNode : BehaviorNodeSource
    {
        [ExpressionSignature(typeof(int))]
        [BlocklySourceProperty("循环次数", 1)]
        public LogicGraph loopCount;

        [BlocklySourceProperty("循环体", 2)]
        public BehaviorNodeSource behavior;

        sealed class Node : CompositeBehavior<LoopNode>
        {
            private LogicGraph.Blockly _loopCount;

            private IBehaviorNode _behavior;

            private int _remainingLoops;

            private bool _running;

            protected override void Initialize()
            {
                _behavior = blockly.CreateBlock(source.behavior);
                if (_behavior == null)
                    throw new Exception($"Failed to create graph node from source: {source.behavior}");

                _loopCount = blockly.CreateBlockly(source.loopCount);
                if (_loopCount == null)
                    throw new Exception($"Failed to create graph node from source: {source.loopCount}");
            }

            protected override void OnStart()
            {
                _running = true;

                int count = _loopCount.Call<int>();

                _remainingLoops = Math.Max(1, count);

                _behavior.Start();
            }

            protected override BehaviorResult OnTick(float deltaTime)
            {
                if (_running && _behavior.Tick(deltaTime) != BehaviorResult.Running)
                {
                    _behavior.Finish();

                    _running = false;

                    _remainingLoops--;

                    if (_remainingLoops > 0)
                    {
                        _behavior.Start();

                        _running = true;
                    }
                }
                return _running ? BehaviorResult.Running : BehaviorResult.Done;
            }

            protected override void OnLateTick(float deltaTime)
            {
            }

            protected override void OnFinish()
            {
                if (_running)
                {
                    _running = false;

                    _behavior.Finish();
                }
            }

            protected override void OnResetData()
            {
                _running = false;

                _remainingLoops = 0;
            }

            protected override void OnDestroy()
            {
                blockly.DestroyBlockly(_loopCount);
                blockly.DestroyBlock(_behavior);

                _loopCount = null;
                _behavior = null;
            }
        }
    }
}
