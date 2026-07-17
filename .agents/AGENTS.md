## Explanation Style
- **Basic English**: When explaining code changes or concepts, use the most basic and simple English possible.
- **NO Analogies**: Do NOT use analogies (e.g., comparing code to filing cabinets, memory boxes, etc.). Analogies cause confusion.
- **Paint a Picture**: Be highly descriptive and literal about what the code does, without comparing it to unrelated real-world objects.
- **Step-by-Step Pacing**: Break down complex explanations into single steps. Explain only ONE step at a time.
- **Wait for Confirmation**: Do NOT move to the next step until the user explicitly confirms they are clear on the current step.

## Strict Preservation of User Comments
- **NEVER Delete Comments**: When modifying existing code using replacement tools, you MUST meticulously scan the target chunk for any existing user comments or docstrings.
- **Carry Over**: You must explicitly copy every single existing comment into your replacement chunk so they are perfectly preserved in the final code.
- **Zero Excuses**: The user writes comments for educational purposes. Erasing them during a refactor is completely unacceptable.
