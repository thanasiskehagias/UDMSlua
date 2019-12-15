-- aliases --
local UE = CS.UnityEngine
local sin = UE.Mathf.Sin
local cos = UE.Mathf.Cos

local anims = {}

local clip1 = 'Crouch Walk02'
local clip2 = 'Jazz Dancing01'

local Nagn = Members.Count

local R0 = 1
local T0, T1, T2 = 400, 0, 300
local v1, w1, a1 = 0.01, 3.14159/100, 1;
local NormTransDur = 0.2
local TIME = 0

function start()
    for i=0,Nagn - 1 do
        anims[i] = Members[i]:GetComponent(typeof(UE.Animator))
    end

    for i=0,Nagn-1 do
        individualPrepare(i)
    end
end

function update()
    for i=0,Nagn-1 do
        individualMove(i)
    end
    TIME = TIME + 1
end

function individualPrepare(agentId) 
    local agent = Members[agentId]

    agent.transform.eulerAngles.y = -90 - (agentId*360)/Nagn
    agent.transform.position = R0 * UE.Vector3(cos(0.1+agentId*6.28/Nagn), 0, sin(0.1+agentId*6.28/Nagn))
    anims[agentId]:Play(clip1, 0, 0)
end

function individualMove(agentId)
    local myAnim = anims[agentId]
    local transform = Members[agentId].transform

    if TIME%T0==T1 then 
        myAnim:CrossFade(clip1, NormTransDur);     -- animation clips change at fixed times.
    end
    if TIME%T0==T2 then
         myAnim:CrossFade(clip2, NormTransDur);
    end

    transform:Rotate(UE.Vector3(0, 1 + cos(w1 * TIME), 0))
    transform:Translate(UE.Vector3.forward * v1)
end

--[[ Root motion is off :)
function onElementAnimatorMove(agentId)
    anims[agentId]:ApplyBuiltinRootMotion()
end
--]]
