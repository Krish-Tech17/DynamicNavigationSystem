🧭 Dynamic Navigation System (Unity)
📌 Overview

This project implements a dynamic, waypoint-based navigation system for Unity.
The system enables users to navigate to selected destinations within a virtual environment while dynamically computing the shortest valid route, intelligently handling Primary and Secondary pathways, and supporting multi-floor navigation.

The navigation logic is designed to adapt in real time based on user movement, navigation rules, and environmental constraints—without requiring scene reloads or system restarts.

🛣️ Primary & Secondary Pathway Support

Calculates routes using Primary pathway networks

Automatically switches to Secondary pathways when:

Primary paths are blocked

User enters a restricted or secondary zone

Context-aware path selection based on the user’s current position

🏢 Multi-Floor Navigation

Supports navigation across multiple floors and levels

Distance calculations account for:

Floor transitions

Level connectors (stairs, elevators, ramps)

Ensures correct routing even when destination is on a different floor

🚧 Blocking & Feedback Mechanisms

Detects when the user exits a valid navigable route

Provides immediate feedback through:

Visual warnings

Audio cues

Suggests an alternative valid route in real time

🔄 Runtime Path Recalculation

Navigation dynamically adapts when:

The user deviates from the guided route

The current path becomes invalid due to:

Rule violations

Virtual blocking

Path updates occur seamlessly
No restart or reinitialization required

🧩 Scene & Testing Details
Demo Scene

Scene Name: DemoFloorTest

Scene Includes

Multi-floor environment setup

Primary and Secondary waypoint networks

Rule-based navigable and restricted zones

Blocking simulation with rerouting logic

Visual and audio feedback triggers

Test Scenarios Covered

Switching between Primary and Secondary paths

Exiting and re-entering navigable zones

Multi-floor path distance validation

Dynamic rerouting during active navigation

📦 Key Highlights

Modular and extensible waypoint architecture

Rule-driven navigation behavior

Scalable for large indoor environments

Suitable for:

VR / MR training simulations

Digital twins

Complex indoor navigation scenarios
