//Global story tags
# title: A Find on the Road
# frequency: Uncommon
# development: false
# illustration: village
# Repeatable: yes
       INCLUDE include.ink
VAR PlayerWin = false
 VAR RewardRoll = 0
           ~ RewardRoll = RANDOM(0,1)
           
        VAR RewardText = ""
            {
                - RewardRoll == 0:
                    ~ RewardText = "Spellcraft and Faith increased"
                - RewardRoll == 1:
                    ~ RewardText = "Spellcraft and Faith increased"
            }
-> Start

===Start===

You are travelling through the hinterland at the head of your company. Wearing their respective uniform and holding up the clan’s banners high, your men march in an almost perfect lockstep with each other. 
A strong gust of wind carries the thunderous sound of their footsteps to distant hills, valleys and woods. It is a good day today. #STR_Start1 

The peaceful scenery is disrupted when you suddenly hear the back ranks of your men arguing.
Upon getting closer, you discover the troops squabbling over a strongbox held by one of the soldiers – who claims it was simply lying on the grass, not that far off the road.  #STR_Start2

They open the chest and you peer inside, it is obvious the item inside is no mere trinket. It would definitely fetch a fair price on the market or could even benefit your goals directly.
However, you have to wonder: Why is a seemingly precious relic lying off the road in the middle of nowhere? #STR_Start3

What is your choice? #STR_Start4

 ->choices1

    =choices1
    *[Tell the men to hand over the object to you.] ->HandOver
    *[Let your men keep the artifact – after all, they found the relic] ->Keep
    *[Ask to see the item and inspect it for otherworldly properties] ->SeeProperties
    
===HandOver===
You order the troops to disperse and hand over the trinket to you but the troops seem hesitant.
Most of the troops move out of the way but the small group that found the strongbox holds firm as you approach them, their sense of duty overshadowed by the trinket. #STR_HandOver1 

The group shuffles around a bit until one finds enough confidence to step forwards.#STR_HandOver2 

“With all due respect m’lord, it was us who found the strongbox, so we figure the trinket should remain with us. Finders keepers and that.”
The man’s hand slowly hovers close to his weapon, although not in a threatening manner: “The way I see it we are not going to reach a conclusion on the matter, how about we duel for it?”  #STR_HandOver3 

You feel the eyes of your men dig into your skull after the words leave his lips. You realise the man before you is not just some common footpad; he is one of the more seasoned veterans of your company.
The proposition is not a bad one, and would even provide the men with some entertainment, but it is not impossible to persuade the men to surrender the treasure with words. #STR_HandOver4 
->choices

    =choices
    +[Try to persuade your troops {print_party_skill_chance("Charm", 150)}]->Persuade
    *[Demand your men to give up the artifact or else any person involved gets 10 lashes.]->DemandArtifact
    *[Invoke your position as the commander {print_party_skill_chance("Steward", 150)}]->Invoke
    *[Accept the challenge and duel for the relic.]->Challenge
    *[Decide it is not worth the effort and let your troops keep the item.]->NotWorth


===Persuade===
{perform_party_skill_check("Charm",150): -> success | -> fail}

 =success
    (SUCCESS)
    Your silver tongue has gotten you out of a few scrapes here and there. Surely, it will do the trick here as well.
	You talk to the men not as a commander but as a comrade in arms, you convince them you are just like them. Highborn or lowborn does not matter in the eyes of the gods after all.
	You crack a few jokes here and there until the men relent and share the trinket with you, how could they refuse such a good friend? #STR_CharmCheckSuccess1
    REWARD: THE ITEM + 5 MORALE
 ->END
 
     =fail
    (FAIL)
You try to convince the men you are a lowborn nobody, just like them. Surprisingly this failed miserably. #STR_CharmCheckFail1
NOTE: -15 MORALE
* [Try something else]->Start.choices1
->END

===DemandArtifact===
The gall of these people! You get in their faces and shout about the fate that awaits them if they refuse to surrender the trinket.#STR_DemandArtifact1

The men turn white, salute and return to their respective units.
The veteran glares at you, spits on the ground and makes his way to his unit, leaving the strongbox on the ground. #STR_DemandArtifact2
~ AddTraitInfluence("Mercy", -20)
NOTE: ADD -50 MORALE AND THE ITEM
->END

===Invoke===
{perform_party_skill_check("steward",150): -> success | -> fail}

=success
   (SUCCESS)
You convey to the men the sense of brotherhood that is 
shared within the company; of battles fought, lives saved and enemies slain. You 
remind them of the times when slights were ignored and other disputes that were 
settled amicably and honourably. You can see their postures shift and they nod in 
agreement, eagerly returning to their duty as they hand you the trinket. #STR_InvokeSuccess1
REWARD: +15 MORALE
->END

=fail
    (FAIL)
    You stumble over your words as you remind the men of brotherhood and sense of duty. The men however remain unconvinced and you can’t help but feel you lost a few inches of height. #STR_InvokeFail1
    NOTE: MORELE -15 AND GO BACK TO LINE 41
    
->END

===Challenge===
You eagerly accept the offer, find a suitable place for a duel and draw your weapon. May the best fighter triumph! #STR_Challenge1
->enterArena

(PIERO'S NOTE: Start a melee 1h duel against the highest tier unit in your party, can take place in the arena where you fight the Duelist event) 

=enterArena
~ OpenDuelMission()
...
{PlayerWin:You sheathe your weapon and wave to your troops, preoccupied with cheering and shouting. The veteran slowly rises to his feet, his eyes are bitter with defeat, but he lets out a bellowing laugh and congratulates you on your victory. He personally hands over the trinket to you.} NOTE:GET THE ITEM #STR_PlayerWin1

{not PlayerWin:You wake up to see the clear skies, the cheering of your men echoing all around you, but they are not cheering for you. You lift your head to see the victor, waving to the crowd and holding the trinket up high. You have lost the duel.} #STR_PlayerWin2

->END

===NotWorth===
You don't give it any importance and continue on your way. There are more crucial matters that require your attention.
->END

===Keep===
You carefully assess the situation, and conclude the recent achievements of your company are more than satisfactory, thus a reward seems appropriate.
The men look at you expectantly and eventually you relent and tell them they can have the item, as long as there is no infighting as to who gets to keep it. #STR_Keep1
(+50 Morale bonus, don’t get the item)  
->END

===SeeProperties===
You ask the men to hand over the item for inspection.
Hesitance has turned into wariness. The men are fully aware of the Chaotic influence certain items can possess. The party has come across the odd priest and witch hunter and know what punishment awaits those who stray from the righteous path.
The men worried glances before they turn the strongbox towards you. #STR_SeeProperties1

{perform_party_skill_check("Spellcraft",150): -> success | -> fail}

=success
   (SUCCESS)


{RewardRoll:
                -0: You utter a few words and concentrate on the relic before you. By focusing your abilities, you manage to grasp faint movements and whispers – a subtle hint of power swirling and coiling within the object. #STR_SeePropertiesSuccessRandom01 
                -> LordNearby
                
                    NOTE: PUT THE MAGICAL ITEM
                -1: You utter a few words and concentrate on the relic before you. By focusing your abilities, you manage to grasp faint movements and whispers – a subtle hint of power swirling and coiling within the object.#STR_SeePropertiesSuccessRandom11
                   NOTE: PUT THE NO-MAGICAL ITEM 
                   * [Decide what to do with this item]->Start.choices1
                }
                
=fail
    (FAIL)
    You try as hard as you can but there seems to be nothing out of the ordinary about this relic.
* [Decide what to do with this item]->Start.choices1
->END

===LordNearby===

->choices

    =choices
    *[Decide to hand over the object to a lord ruling the nearby settlement.]->Noble
    *[(Lie) Loudly announce this magical trinket can be dangerous to its wielder and you are confiscating it for safekeeping.]->Lie
    
 ===Noble===   
You decide dealing with artifacts of unknown origin is too much of a risk. The troops are ordered to safely stash the item to be relinquished to authorities at the earliest opportunity. #STR_Noble1

Your men nod with approval – none dare oppose your verdict, especially when it comes to dubious magical trinkets.#STR_Noble2
 ~ GiveGold(10000)
 NONTE:ADD 2000 FAITH EXP, +15 RELATION BONUS WITH YOUR FACTION CLANS AND LOSE THE ITEM

->END

===Lie===   
   You announce that the relic is influenced with the powers of Chaos, proclaiming you can hear whispers coming from the strongbox. You explain to the men how dangerous it can be for the owner of the item.
   You also invoke your experience with relics like this and request the men to leave the trinket with you. The troops know well enough not to dabble with such foul forces. It is no secret one would have to be a raving lunatic to take advantage of questionable magicks. #STR_Lie1
   
You feel a bizarre aura of triumph clouding your thoughts. Or is it simply one’s imagination playing tricks?
It has been a long day after all. Either way you feel a grin slowly creeping on your face and prepare to resume your journey. #STR_Lie2

PIERO'S NOTE: Potential  corruption points, not implemented yet, get the item.
->END
