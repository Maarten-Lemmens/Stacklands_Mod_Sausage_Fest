using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;

namespace SausageFestModNS
{
    public class SausageFestMod : Mod
    {
        public override void Ready()
        {
            WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.AdvancedBuildingIdea, "sausagefestmod_blueprint_slaughterhouse", 1);
			WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.AdvancedBuildingIdea, "sausagefestmod_blueprint_meat_grinder", 1);
			WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.AdvancedFood, "sausagefestmod_blueprint_cooked_sausage", 1);
			WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.AdvancedFood, "sausagefestmod_blueprint_cooked_blood_sausage", 1);
			WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.AdvancedFood, "sausagefestmod_blueprint_raw_sausage", 1);
			WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.AdvancedFood, "sausagefestmod_blueprint_raw_blood_sausage", 1);
			WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.AdvancedFood, "sausagefestmod_blueprint_blutwurst_beer", 1);
			WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.AdvancedFood, "sausagefestmod_blueprint_wurst_beer", 1);

			/*
			* Define SausageFestQuest
			*/
			//Quest sausageFestQuest = new Quest()
			//QuestManager.instance.CurrentQuests.Add(sausageFestQuest);

			Logger.Log("Thorrific's Sausage Fest is now ready! Bon appetit ;)");
        }
    }

	/*
	* Had to rename SlaughterHouse into SausageFestSlaugtherhous to make a distinction with the default Stacklands 'Butchery' (which is actually called SlaughterHouse already in the Stacklands implementation...)
	*/
	public class SausageFestSlaughterhouse : SlaughterHouse
	{
		public override bool DetermineCanHaveCardsWhenIsRoot => true;
				
		public override bool CanHaveCardsWhileHasStatus()
		{
			return true;
		}

		// this method decides whether a card should stack onto this one
		protected override bool CanHaveCard(CardData otherCard)
		{
			int num = GetChildCount() + (1 + otherCard.GetChildCount());
			if (otherCard.Id == "cow")
			{
				return num <= 5;
			}
			return false;
		}

		// this method is called every frame, it is the CardData equivalent of the Update method
		public override void UpdateCard()
		{
			if (MyGameCard.HasChild && MyGameCard.Child.CardData is Animal)
			{
				MyGameCard.StartTimer(30f, SlaughteringAnimal, SokLoc.Translate("sausagefestmod_action_slaughtering_status"), GetActionId("SlaughteringAnimal"));
			}
			else
			{
				MyGameCard.CancelTimer(GetActionId("SlaughteringAnimal"));
			}
			//base.UpdateCard();
		}

		[TimedAction("slaughtering_animal")]
		public void SlaughteringAnimal()
		{
			if (MyGameCard.HasChild && MyGameCard.Child.CardData is Animal)
			{
				GameCard child = MyGameCard.Child;
				RemoveFirstChildFromStack();
				child.DestroyCard();
				// Raw meat
				CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "raw_meat", checkAddToStack: false);
				WorldManager.instance.StackSendCheckTarget(MyGameCard, cardData.MyGameCard, OutputDir);
				// Organ meat
				cardData = WorldManager.instance.CreateCard(base.transform.position, "sausagefestmod_organ_meat", checkAddToStack: false);
				WorldManager.instance.StackSendCheckTarget(MyGameCard, cardData.MyGameCard, OutputDir); // required to make organ meat auto-stack if there is a card in the vicinity.
				// Blood
				cardData = WorldManager.instance.CreateCard(base.transform.position, "sausagefestmod_blood", checkAddToStack: false);
				WorldManager.instance.StackSendCheckTarget(MyGameCard, cardData.MyGameCard, OutputDir); // required to make blood auto-stack if there is a card in the vicinity.

				WorldManager.instance.CreateSmoke(base.transform.position);
			}
		}
	}

	public class SausageFestMeatGrinder : CardData
	{
		// this method decides whether a card should stack onto this one
		protected override bool CanHaveCard(CardData otherCard)
		{
			if (otherCard.Id != "sausagefestmod_organ_meat" && otherCard.Id != "sausagefestmod_blood")
				return false;
			return true;
		}
		
		// this method is called every frame, it is the CardData equivalent of the Update method
		public override void UpdateCard()
		{
			base.UpdateCard();
		} 

		public override bool CanHaveCardsWhileHasStatus()
		{
			return true;
		}
	}




	/*
	* Delirium tremens (DTs) is a severe form of alcohol withdrawal (existing in real life)
	* that typically occurs in people who have been drinking heavily for an extended period 
	* and then suddenly stop or significantly reduce their alcohol intake.
	*
	* It is characterized by sudden and severe mental or nervous system changes.
	* It is sometimes represented as seeing pink elephants.
	*
	* The "delirium tremes"-mob is a conceptual representation of the above.
	* - It is angry & aggro when it occurs, wrecking havoc --> Mob
	* - It can be "tamed" by feeding it strong beer, causing it to get drunk --> Animal
	* - It becomes "aggro" again once it sobers up --> Mob.
	*
	* It's represented as two different creatures below.
	* 
	*/
	
	/*
	* Create a class called DeliriumTremensWild which extends the Mob class --> wild (unfriendly) version
	* It has chance to spawn, each time when a villager sobers up.
	* It can be taimed by providing it a wurst beer.
	*/
	public class DeliriumTremensWild : Mob
	{
		public override void OnInitialCreate()
		{
			HealthPoints = 150;
			base.OnInitialCreate();
		}
		
		// this method decides whether a card should stack onto this one
		protected override bool CanHaveCard(CardData otherCard)
		{
			if (otherCard.Id == "sausagefestmod_wurst_beer" || otherCard.Id == "sausagefestmod_blutwurst_beer")
			{
				return true;
			}
			return base.CanHaveCard(otherCard); // otherwise, we will let Animal.CanHaveCard decide
		}
		
		// this method is called every frame, it is the CardData equivalent of the Update method
		public override void UpdateCard()
		{
			if (MyGameCard.HasChild && (MyGameCard.Child.CardData.Id == "sausagefestmod_wurst_beer" || MyGameCard.Child.CardData.Id == "sausagefestmod_blutwurst_beer"))
			{
				MyGameCard.StartTimer(5f, DrinkingWurstBeer, SokLoc.Translate("sausagefestmod_action_drinking_wurstbeer_status"), GetActionId("DrinkingWurstBeer"));
			}
			else
			{
				MyGameCard.CancelTimer(GetActionId("DrinkingWurstBeer"));
			}
			base.UpdateCard();
		}

		[TimedAction("drinking_beer")]
		public void DrinkingWurstBeer()
		{
			if (MyGameCard.HasChild)
			{
				if (MyGameCard.Child.CardData.Id == "sausagefestmod_wurst_beer") // change into a friendly version
				{
					CardData cardData = WorldManager.instance.ChangeToCard(MyGameCard, "sausagefestmod_delirium_tremens_tamed");
					MyGameCard.Child.DestroyCard(); // destroy consumed beer card
					WorldManager.instance.CreateSmoke(cardData.transform.position);
					cardData.AddStatusEffect(new StatusEffect_Drunk());
					cardData.MyGameCard.SendIt();
				}
				else if (MyGameCard.Child.CardData.Id == "sausagefestmod_blutwurst_beer") // become frenzy
				{
					MyGameCard.Child.DestroyCard(); // destroy consumed beer card
					WorldManager.instance.CreateSmoke(MyGameCard.transform.position);
					RemoveAllStatusEffects(); // remove drunk if it was still there
					AddStatusEffect(new StatusEffect_Frenzy());
				}
			}
		}
	}

	/*
	* Create a class called DeliriumTremensTamed which extends the Mob class --> friendly (drunk) version
	* It spawns when the aggro version is fed the right type of alcohol.
	*/
	public class DeliriumTremensTamed : Animal
	{
		// this method decides whether a card should stack onto this one
		protected override bool CanHaveCard(CardData otherCard)
		{
			if (otherCard.Id == "sausagefestmod_wurst_beer" || otherCard.Id == "sausagefestmod_blutwurst_beer")
			{
				return true;
			}
			return base.CanHaveCard(otherCard); // otherwise, we will let Animal.CanHaveCard decide
		}
		
		// this method is called every frame, it is the CardData equivalent of the Update method
		public override void UpdateCard()
		{
			if (MyGameCard.HasChild && (MyGameCard.Child.CardData.Id == "sausagefestmod_wurst_beer" || MyGameCard.Child.CardData.Id == "sausagefestmod_blutwurst_beer"))
			{
				MyGameCard.StartTimer(5f, DrinkingWurstBeer, SokLoc.Translate("sausagefestmod_action_drinking_wurstbeer_status"), GetActionId("DrinkingWurstBeer"));
			}
			else
			{
				MyGameCard.CancelTimer(GetActionId("DrinkingWurstBeer"));
			}
			base.UpdateCard();
		}

		[TimedAction("drinking_beer")]
		public void DrinkingWurstBeer()
		{
			if (MyGameCard.HasChild && MyGameCard.Child.CardData.Id == "sausagefestmod_blutwurst_beer") // change into a aggro version with ***Blut*** Wurst Beer
			{
				CardData cardData = WorldManager.instance.ChangeToCard(MyGameCard, "sausagefestmod_delirium_tremens_wild");
				MyGameCard.Child.DestroyCard(); // destroy consumed beer-card
				WorldManager.instance.CreateSmoke(cardData.transform.position);
				cardData.RemoveAllStatusEffects(); // To remove drunk, if it was still there
				cardData.AddStatusEffect(new StatusEffect_Frenzy()); // becomes frenzy
				cardData.MyGameCard.SendIt();
			}
			else if (MyGameCard.HasChild && MyGameCard.Child.CardData.Id == "sausagefestmod_wurst_beer") //remove wurst beer & keep drunk
			{
				MyGameCard.Child.DestroyCard(); // destroy consumed beer card
				WorldManager.instance.CreateSmoke(MyGameCard.transform.position);
				
				// Refresh state of drunk-ness (first remove old, then add new)
				RemoveAllStatusEffects();
				AddStatusEffect(new StatusEffect_Drunk());
			}
		}
	}


	/*
	* The (blut) wurst beer can be put:
	* - either on a villager, that will drink the beer, and when consumed potentially spawns delirium tremes mob.
	* - either on the delrium tremes mob itself, that will drink the beer
	* - - if delerium tremes consumes a regular "beer wurst", then there is a chance that it becomes friendly.
	* - - if delirum tremes consumes a blood "beer wurst", then it becomes hostile.
	*/
	public class SausageFestWurstBeer : Food
	{
		// this method is called every frame, it is the CardData equivalent of the Update method
		public override void UpdateCard()
		{
			if (MyGameCard.HasParent)
			{
				if (MyGameCard.Parent.CardData is BaseVillager)
				{
					MyGameCard.StartTimer(5f, DrinkingWurstBeer, SokLoc.Translate("sausagefestmod_action_drinking_wurstbeer_status"), GetActionId("DrinkingWurstBeer"));
				}
			}
			else
			{
				MyGameCard.CancelTimer(GetActionId("DrinkingWurstBeer"));
			}
			base.UpdateCard();
		}

		[TimedAction("drinking_beer")]
		public void DrinkingWurstBeer()
		{
			if (MyGameCard.HasParent)
			{
				// If the (blood) wurst beer is consumed by a villager, potentially spawn a delirium tremes mob
				if (MyGameCard.Parent.CardData is BaseVillager)
				{
					WorldManager.instance.CreateSmoke(base.transform.position);
					MyGameCard.Parent.CardData.RemoveAllStatusEffects();
					MyGameCard.Parent.CardData.AddStatusEffect(new StatusEffect_Drunk()); // Make villager drunk
					MyGameCard.DestroyCard(); // Destroy consumed beer card
					AudioManager.me.PlaySound(AudioManager.me.Eat, Position); // play the eating sound at the delirum tremens position	

					if (UnityEngine.Random.Range(0, 100) < 5) // 5% chance to summon delirium tremens (wild)
					{
						CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "sausagefestmod_delirium_tremens_wild", checkAddToStack: false);
						cardData.MyGameCard.SendIt();
						//WorldManager.instance.StackSendCheckTarget(MyGameCard, cardData.MyGameCard, OutputDir);
					}
				}
			}
		}
	}

/*
	public class SausageFestQuest : Quest
	{
		
		//public string Id => "SausageFestQuest";
		public Location QuestLocation
		{
			return Location.Mainland.Value;
		}

		public bool IsMainQuest
		{
			return true; // to check
		}

		//public string Id;


		public Quest(string id)
		{
			Id = id;
		}
	}
	*/

/*
	public class VillagerDwarfScout : Villager
	{
		public override void OnInitialCreate()
		{
			HealthPoints = 15;
			base.OnInitialCreate();
		}
	}
	*/
}