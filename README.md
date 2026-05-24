# Mobius-Engineering-Code-Sample
Here is a short code sample for Mobius's Software Engineering position.

## System Context

This showcases the use of a modular player ability system. 

This system was designed to allow abilities to be dragged and dropped onto a player while requiring minimal extra setup.

The system allows disabling player abilities based on game context (for example if the player is falling we can disable the jump ability)

## Abstractions

While the player is an abstraction of an Entity in this example, the abilities of entities and players are not abstracted since both players and entities treat abilities fundementally differently.

## Code & Comments

The sample includes 4 classes all squashed down into a single file for ease of review. Namespaces are also not included to make it easier to follow

A few comments are specifically added to help with the review of this sample and to explain some of the decision behind each system 

## Language and Engine 

This code is using c# and was written for Unity 6.4

## Coding Standards for this project

Variable casing
private, protected, and SerializedFields: pascalCase
public : 