// StringLiteralCacheTests.Literals.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

namespace GolibTests;

public partial class StringLiteralCacheTests
{
    // 600 distinct C# string LITERALS (each element is an ldstr, so a frozen, interned instance), more
    // than the initial table holds at half load: converting them concurrently forces growth under
    // contention. The "plc-" prefix keeps them distinct from every other literal in the test assembly.
    private static readonly string[] s_literals =
    [
        "plc-grow-000", "plc-grow-001", "plc-grow-002", "plc-grow-003", "plc-grow-004", "plc-grow-005", "plc-grow-006", "plc-grow-007", "plc-grow-008", "plc-grow-009",
        "plc-grow-010", "plc-grow-011", "plc-grow-012", "plc-grow-013", "plc-grow-014", "plc-grow-015", "plc-grow-016", "plc-grow-017", "plc-grow-018", "plc-grow-019",
        "plc-grow-020", "plc-grow-021", "plc-grow-022", "plc-grow-023", "plc-grow-024", "plc-grow-025", "plc-grow-026", "plc-grow-027", "plc-grow-028", "plc-grow-029",
        "plc-grow-030", "plc-grow-031", "plc-grow-032", "plc-grow-033", "plc-grow-034", "plc-grow-035", "plc-grow-036", "plc-grow-037", "plc-grow-038", "plc-grow-039",
        "plc-grow-040", "plc-grow-041", "plc-grow-042", "plc-grow-043", "plc-grow-044", "plc-grow-045", "plc-grow-046", "plc-grow-047", "plc-grow-048", "plc-grow-049",
        "plc-grow-050", "plc-grow-051", "plc-grow-052", "plc-grow-053", "plc-grow-054", "plc-grow-055", "plc-grow-056", "plc-grow-057", "plc-grow-058", "plc-grow-059",
        "plc-grow-060", "plc-grow-061", "plc-grow-062", "plc-grow-063", "plc-grow-064", "plc-grow-065", "plc-grow-066", "plc-grow-067", "plc-grow-068", "plc-grow-069",
        "plc-grow-070", "plc-grow-071", "plc-grow-072", "plc-grow-073", "plc-grow-074", "plc-grow-075", "plc-grow-076", "plc-grow-077", "plc-grow-078", "plc-grow-079",
        "plc-grow-080", "plc-grow-081", "plc-grow-082", "plc-grow-083", "plc-grow-084", "plc-grow-085", "plc-grow-086", "plc-grow-087", "plc-grow-088", "plc-grow-089",
        "plc-grow-090", "plc-grow-091", "plc-grow-092", "plc-grow-093", "plc-grow-094", "plc-grow-095", "plc-grow-096", "plc-grow-097", "plc-grow-098", "plc-grow-099",
        "plc-grow-100", "plc-grow-101", "plc-grow-102", "plc-grow-103", "plc-grow-104", "plc-grow-105", "plc-grow-106", "plc-grow-107", "plc-grow-108", "plc-grow-109",
        "plc-grow-110", "plc-grow-111", "plc-grow-112", "plc-grow-113", "plc-grow-114", "plc-grow-115", "plc-grow-116", "plc-grow-117", "plc-grow-118", "plc-grow-119",
        "plc-grow-120", "plc-grow-121", "plc-grow-122", "plc-grow-123", "plc-grow-124", "plc-grow-125", "plc-grow-126", "plc-grow-127", "plc-grow-128", "plc-grow-129",
        "plc-grow-130", "plc-grow-131", "plc-grow-132", "plc-grow-133", "plc-grow-134", "plc-grow-135", "plc-grow-136", "plc-grow-137", "plc-grow-138", "plc-grow-139",
        "plc-grow-140", "plc-grow-141", "plc-grow-142", "plc-grow-143", "plc-grow-144", "plc-grow-145", "plc-grow-146", "plc-grow-147", "plc-grow-148", "plc-grow-149",
        "plc-grow-150", "plc-grow-151", "plc-grow-152", "plc-grow-153", "plc-grow-154", "plc-grow-155", "plc-grow-156", "plc-grow-157", "plc-grow-158", "plc-grow-159",
        "plc-grow-160", "plc-grow-161", "plc-grow-162", "plc-grow-163", "plc-grow-164", "plc-grow-165", "plc-grow-166", "plc-grow-167", "plc-grow-168", "plc-grow-169",
        "plc-grow-170", "plc-grow-171", "plc-grow-172", "plc-grow-173", "plc-grow-174", "plc-grow-175", "plc-grow-176", "plc-grow-177", "plc-grow-178", "plc-grow-179",
        "plc-grow-180", "plc-grow-181", "plc-grow-182", "plc-grow-183", "plc-grow-184", "plc-grow-185", "plc-grow-186", "plc-grow-187", "plc-grow-188", "plc-grow-189",
        "plc-grow-190", "plc-grow-191", "plc-grow-192", "plc-grow-193", "plc-grow-194", "plc-grow-195", "plc-grow-196", "plc-grow-197", "plc-grow-198", "plc-grow-199",
        "plc-grow-200", "plc-grow-201", "plc-grow-202", "plc-grow-203", "plc-grow-204", "plc-grow-205", "plc-grow-206", "plc-grow-207", "plc-grow-208", "plc-grow-209",
        "plc-grow-210", "plc-grow-211", "plc-grow-212", "plc-grow-213", "plc-grow-214", "plc-grow-215", "plc-grow-216", "plc-grow-217", "plc-grow-218", "plc-grow-219",
        "plc-grow-220", "plc-grow-221", "plc-grow-222", "plc-grow-223", "plc-grow-224", "plc-grow-225", "plc-grow-226", "plc-grow-227", "plc-grow-228", "plc-grow-229",
        "plc-grow-230", "plc-grow-231", "plc-grow-232", "plc-grow-233", "plc-grow-234", "plc-grow-235", "plc-grow-236", "plc-grow-237", "plc-grow-238", "plc-grow-239",
        "plc-grow-240", "plc-grow-241", "plc-grow-242", "plc-grow-243", "plc-grow-244", "plc-grow-245", "plc-grow-246", "plc-grow-247", "plc-grow-248", "plc-grow-249",
        "plc-grow-250", "plc-grow-251", "plc-grow-252", "plc-grow-253", "plc-grow-254", "plc-grow-255", "plc-grow-256", "plc-grow-257", "plc-grow-258", "plc-grow-259",
        "plc-grow-260", "plc-grow-261", "plc-grow-262", "plc-grow-263", "plc-grow-264", "plc-grow-265", "plc-grow-266", "plc-grow-267", "plc-grow-268", "plc-grow-269",
        "plc-grow-270", "plc-grow-271", "plc-grow-272", "plc-grow-273", "plc-grow-274", "plc-grow-275", "plc-grow-276", "plc-grow-277", "plc-grow-278", "plc-grow-279",
        "plc-grow-280", "plc-grow-281", "plc-grow-282", "plc-grow-283", "plc-grow-284", "plc-grow-285", "plc-grow-286", "plc-grow-287", "plc-grow-288", "plc-grow-289",
        "plc-grow-290", "plc-grow-291", "plc-grow-292", "plc-grow-293", "plc-grow-294", "plc-grow-295", "plc-grow-296", "plc-grow-297", "plc-grow-298", "plc-grow-299",
        "plc-grow-300", "plc-grow-301", "plc-grow-302", "plc-grow-303", "plc-grow-304", "plc-grow-305", "plc-grow-306", "plc-grow-307", "plc-grow-308", "plc-grow-309",
        "plc-grow-310", "plc-grow-311", "plc-grow-312", "plc-grow-313", "plc-grow-314", "plc-grow-315", "plc-grow-316", "plc-grow-317", "plc-grow-318", "plc-grow-319",
        "plc-grow-320", "plc-grow-321", "plc-grow-322", "plc-grow-323", "plc-grow-324", "plc-grow-325", "plc-grow-326", "plc-grow-327", "plc-grow-328", "plc-grow-329",
        "plc-grow-330", "plc-grow-331", "plc-grow-332", "plc-grow-333", "plc-grow-334", "plc-grow-335", "plc-grow-336", "plc-grow-337", "plc-grow-338", "plc-grow-339",
        "plc-grow-340", "plc-grow-341", "plc-grow-342", "plc-grow-343", "plc-grow-344", "plc-grow-345", "plc-grow-346", "plc-grow-347", "plc-grow-348", "plc-grow-349",
        "plc-grow-350", "plc-grow-351", "plc-grow-352", "plc-grow-353", "plc-grow-354", "plc-grow-355", "plc-grow-356", "plc-grow-357", "plc-grow-358", "plc-grow-359",
        "plc-grow-360", "plc-grow-361", "plc-grow-362", "plc-grow-363", "plc-grow-364", "plc-grow-365", "plc-grow-366", "plc-grow-367", "plc-grow-368", "plc-grow-369",
        "plc-grow-370", "plc-grow-371", "plc-grow-372", "plc-grow-373", "plc-grow-374", "plc-grow-375", "plc-grow-376", "plc-grow-377", "plc-grow-378", "plc-grow-379",
        "plc-grow-380", "plc-grow-381", "plc-grow-382", "plc-grow-383", "plc-grow-384", "plc-grow-385", "plc-grow-386", "plc-grow-387", "plc-grow-388", "plc-grow-389",
        "plc-grow-390", "plc-grow-391", "plc-grow-392", "plc-grow-393", "plc-grow-394", "plc-grow-395", "plc-grow-396", "plc-grow-397", "plc-grow-398", "plc-grow-399",
        "plc-grow-400", "plc-grow-401", "plc-grow-402", "plc-grow-403", "plc-grow-404", "plc-grow-405", "plc-grow-406", "plc-grow-407", "plc-grow-408", "plc-grow-409",
        "plc-grow-410", "plc-grow-411", "plc-grow-412", "plc-grow-413", "plc-grow-414", "plc-grow-415", "plc-grow-416", "plc-grow-417", "plc-grow-418", "plc-grow-419",
        "plc-grow-420", "plc-grow-421", "plc-grow-422", "plc-grow-423", "plc-grow-424", "plc-grow-425", "plc-grow-426", "plc-grow-427", "plc-grow-428", "plc-grow-429",
        "plc-grow-430", "plc-grow-431", "plc-grow-432", "plc-grow-433", "plc-grow-434", "plc-grow-435", "plc-grow-436", "plc-grow-437", "plc-grow-438", "plc-grow-439",
        "plc-grow-440", "plc-grow-441", "plc-grow-442", "plc-grow-443", "plc-grow-444", "plc-grow-445", "plc-grow-446", "plc-grow-447", "plc-grow-448", "plc-grow-449",
        "plc-grow-450", "plc-grow-451", "plc-grow-452", "plc-grow-453", "plc-grow-454", "plc-grow-455", "plc-grow-456", "plc-grow-457", "plc-grow-458", "plc-grow-459",
        "plc-grow-460", "plc-grow-461", "plc-grow-462", "plc-grow-463", "plc-grow-464", "plc-grow-465", "plc-grow-466", "plc-grow-467", "plc-grow-468", "plc-grow-469",
        "plc-grow-470", "plc-grow-471", "plc-grow-472", "plc-grow-473", "plc-grow-474", "plc-grow-475", "plc-grow-476", "plc-grow-477", "plc-grow-478", "plc-grow-479",
        "plc-grow-480", "plc-grow-481", "plc-grow-482", "plc-grow-483", "plc-grow-484", "plc-grow-485", "plc-grow-486", "plc-grow-487", "plc-grow-488", "plc-grow-489",
        "plc-grow-490", "plc-grow-491", "plc-grow-492", "plc-grow-493", "plc-grow-494", "plc-grow-495", "plc-grow-496", "plc-grow-497", "plc-grow-498", "plc-grow-499",
        "plc-grow-500", "plc-grow-501", "plc-grow-502", "plc-grow-503", "plc-grow-504", "plc-grow-505", "plc-grow-506", "plc-grow-507", "plc-grow-508", "plc-grow-509",
        "plc-grow-510", "plc-grow-511", "plc-grow-512", "plc-grow-513", "plc-grow-514", "plc-grow-515", "plc-grow-516", "plc-grow-517", "plc-grow-518", "plc-grow-519",
        "plc-grow-520", "plc-grow-521", "plc-grow-522", "plc-grow-523", "plc-grow-524", "plc-grow-525", "plc-grow-526", "plc-grow-527", "plc-grow-528", "plc-grow-529",
        "plc-grow-530", "plc-grow-531", "plc-grow-532", "plc-grow-533", "plc-grow-534", "plc-grow-535", "plc-grow-536", "plc-grow-537", "plc-grow-538", "plc-grow-539",
        "plc-grow-540", "plc-grow-541", "plc-grow-542", "plc-grow-543", "plc-grow-544", "plc-grow-545", "plc-grow-546", "plc-grow-547", "plc-grow-548", "plc-grow-549",
        "plc-grow-550", "plc-grow-551", "plc-grow-552", "plc-grow-553", "plc-grow-554", "plc-grow-555", "plc-grow-556", "plc-grow-557", "plc-grow-558", "plc-grow-559",
        "plc-grow-560", "plc-grow-561", "plc-grow-562", "plc-grow-563", "plc-grow-564", "plc-grow-565", "plc-grow-566", "plc-grow-567", "plc-grow-568", "plc-grow-569",
        "plc-grow-570", "plc-grow-571", "plc-grow-572", "plc-grow-573", "plc-grow-574", "plc-grow-575", "plc-grow-576", "plc-grow-577", "plc-grow-578", "plc-grow-579",
        "plc-grow-580", "plc-grow-581", "plc-grow-582", "plc-grow-583", "plc-grow-584", "plc-grow-585", "plc-grow-586", "plc-grow-587", "plc-grow-588", "plc-grow-589",
        "plc-grow-590", "plc-grow-591", "plc-grow-592", "plc-grow-593", "plc-grow-594", "plc-grow-595", "plc-grow-596", "plc-grow-597", "plc-grow-598", "plc-grow-599",
    ];
}
