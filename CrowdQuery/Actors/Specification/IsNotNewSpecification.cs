﻿using Akkatecture.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowdQuery.Actors.Specification
{
	public class IsNotNewSpecification : Specification<bool>
	{
		protected override IEnumerable<string> IsNotSatisfiedBecause(bool aggregate)
		{
			if (aggregate)
			{
				yield return "Aggregate is new";
			}
		}
	}
}
