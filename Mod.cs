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
			Logger.Log("SausageFestMod is now ready!");
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
}