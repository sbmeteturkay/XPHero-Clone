using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Upgrade", menuName = "XP Hero/Base Upgrade", order = 0)]
public abstract class BaseUpgradeData : ScriptableObject
{
    [Header("UI Information")]
    [SerializeField] protected string upgradeName = "Upgrade Name";
    [SerializeField, TextArea(2, 4)] protected string description = "Upgrade description";
    [SerializeField] protected Sprite icon;
    [SerializeField] protected Color upgradeColor = Color.white;
    
    [Header("Upgrade Configuration")]
    [SerializeField] protected int maxUpgradeLevel = 100;
    [SerializeField] protected bool isPlayerLevelDependent = false;
    
    [Header("Formula Verification")]
    [SerializeField] protected TestCase[] verificationData = new TestCase[0];
    
    [Header("Debug")]
    [SerializeField] protected bool showDebugLogs = false;
    
    // Properties
    public string UpgradeName => upgradeName;
    public string Description => description;
    public Sprite Icon => icon;
    public Color UpgradeColor => upgradeColor;
    public int MaxUpgradeLevel => maxUpgradeLevel;
    public bool IsPlayerLevelDependent => isPlayerLevelDependent;
    
    // Test Case Structure
    [System.Serializable]
    public struct TestCase
    {
        [Header("Input Parameters")]
        public int upgradeLevel;
        public int playerLevel;
        
        [Header("Expected Results")]
        public int expectedValue;
        public int expectedCost;
        
        [Header("Verification")]
        public bool isVerified;
        public string notes;
        
        public TestCase(int uLevel, int pLevel, int expValue, int expCost, bool verified = false, string testNotes = "")
        {
            upgradeLevel = uLevel;
            playerLevel = pLevel;
            expectedValue = expValue;
            expectedCost = expCost;
            isVerified = verified;
            notes = testNotes;
        }
    }
    
    // Abstract Methods - Her upgrade türü kendi implementasyonunu yapacak
    public abstract int CalculateValue(int upgradeLevel, int playerLevel = 1);
    public abstract int CalculateCost(int upgradeLevel);
    public abstract string GetValueText(int upgradeLevel, int playerLevel = 1);
    public abstract string GetNextLevelPreview(int currentLevel, int playerLevel = 1);
    
    // Virtual Methods - Override edilebilir
    public virtual bool CanUpgrade(int currentLevel, int playerLevel, int currentCurrency)
    {
        if (currentLevel >= maxUpgradeLevel) return false;
        int cost = CalculateCost(currentLevel + 1);
        return currentCurrency >= cost;
    }
    
    public virtual string GetCostText(int upgradeLevel)
    {
        int cost = CalculateCost(upgradeLevel);
        return FormatNumber(cost);
    }
    
    public virtual float GetUpgradeProgress(int currentLevel)
    {
        return Mathf.Clamp01((float)currentLevel / maxUpgradeLevel);
    }
    
    // Verification System
    [ContextMenu("Verify All Test Cases")]
    public void VerifyAllTestCases()
    {
        if (verificationData == null || verificationData.Length == 0)
        {
            Debug.LogWarning($"[{upgradeName}] No test cases found for verification.");
            return;
        }
        
        int passedTests = 0;
        int totalTests = verificationData.Length;
        
        for (int i = 0; i < verificationData.Length; i++)
        {
            bool testPassed = VerifyTestCase(i);
            if (testPassed) passedTests++;
        }
        
        Debug.Log($"[{upgradeName}] Verification Complete: {passedTests}/{totalTests} tests passed.");
    }
    
    public bool VerifyTestCase(int testIndex)
    {
        if (testIndex < 0 || testIndex >= verificationData.Length) return false;
        
        TestCase testCase = verificationData[testIndex];
        
        // Value verification
        int calculatedValue = CalculateValue(testCase.upgradeLevel, testCase.playerLevel);
        bool valueMatch = calculatedValue == testCase.expectedValue;
        
        // Cost verification
        int calculatedCost = CalculateCost(testCase.upgradeLevel);
        bool costMatch = calculatedCost == testCase.expectedCost;
        
        bool testPassed = valueMatch && costMatch;
        
        if (showDebugLogs || !testPassed)
        {
            string result = testPassed ? "PASS" : "FAIL";
            string log = $"[{upgradeName}] Test {testIndex + 1}: {result}\n" +
                        $"Level: {testCase.upgradeLevel}, Player: {testCase.playerLevel}\n" +
                        $"Value - Expected: {testCase.expectedValue}, Got: {calculatedValue} {(valueMatch ? "✓" : "✗")}\n" +
                        $"Cost - Expected: {testCase.expectedCost}, Got: {calculatedCost} {(costMatch ? "✓" : "✗")}";
            
            if (testPassed && showDebugLogs) Debug.Log(log);
            else if (!testPassed) Debug.LogError(log);
        }
        
        return testPassed;
    }
    
    // Helper Methods
    protected string FormatNumber(int number)
    {
        if (number >= 1000000)
            return $"{number / 1000000.0f:F1}M";
        else if (number >= 1000)
            return $"{number / 1000.0f:F1}k";
        else
            return number.ToString();
    }
    
    protected void DebugLog(string message)
    {
        if (showDebugLogs)
            Debug.Log($"[{upgradeName}] {message}");
    }
    
    // Editor Helper - Test case ekleme
    #if UNITY_EDITOR
    [ContextMenu("Add Test Case")]
    public void AddTestCase()
    {
        var newTestCase = new TestCase(1, 1, 0, 0, false, "New test case");
        var list = new System.Collections.Generic.List<TestCase>(verificationData);
        list.Add(newTestCase);
        verificationData = list.ToArray();
        UnityEditor.EditorUtility.SetDirty(this);
    }
    
    [ContextMenu("Clear All Test Cases")]
    public void ClearTestCases()
    {
        verificationData = new TestCase[0];
        UnityEditor.EditorUtility.SetDirty(this);
    }
    #endif
}

// Extensions and Utilities
public static class UpgradeDataExtensions 
{
    public static string GetDetailedInfo(this BaseUpgradeData upgrade, int currentLevel, int playerLevel)
    {
        string info = $"<b>{upgrade.UpgradeName}</b>\n";
        info += $"{upgrade.Description}\n\n";
        info += $"Current Value: {upgrade.GetValueText(currentLevel, playerLevel)}\n";
        
        if (currentLevel < upgrade.MaxUpgradeLevel)
        {
            info += $"Next Level: {upgrade.GetNextLevelPreview(currentLevel, playerLevel)}\n";
            info += $"Upgrade Cost: {upgrade.GetCostText(currentLevel + 1)}";
        }
        else
        {
            info += "<color=yellow>MAX LEVEL</color>";
        }
        
        return info;
    }
}