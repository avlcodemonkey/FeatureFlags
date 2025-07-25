// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "<Pending>", Scope = "module")]
[assembly: SuppressMessage("Critical Code Smell", "S6967:ModelState.IsValid should be called in controller actions", Justification = "Sonar is tagging this incorrectly.", Scope = "module")]
