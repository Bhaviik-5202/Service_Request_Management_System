// Initial seed data
const initialRequests = [
  {
    id: "1",
    no: "SR-2026-1041",
    title: "Laptop not booting after update",
    description:
      "My laptop shows a blue screen after the latest Windows update. I have an important client demo tomorrow and need this resolved urgently.",
    serviceType: "Technical",
    requestType: "Computer Issue",
    department: "IT",
    requester: "Priya Sharma",
    requesterEmail: "requestor@gmail.com",
    assignee: "Ronak",
    status: "In Progress",
    priority: "Critical",
    created: "2026-07-06T09:12:00",
    updated: "2026-07-07T14:30:00",
    replies: [
      {
        id: 1,
        author: "Ronak",
        role: "Technician",
        message: "Picked this up. Running startup repair now — will update within the hour.",
        date: "2026-07-06T10:05:00",
        status: "In Progress",
      },
      {
        id: 2,
        author: "Priya Sharma",
        role: "Requestor",
        message: "Thanks! Please prioritize, demo is at 10 AM tomorrow.",
        date: "2026-07-06T10:22:00",
      },
      {
        id: 3,
        author: "Ronak",
        role: "Technician",
        message: "Corrupted driver identified. Rolling back and testing.",
        date: "2026-07-07T14:30:00",
        status: "In Progress",
      },
    ],
    timeline: [
      {
        id: "t1",
        status: "Pending",
        changedBy: "Priya Sharma",
        changedAt: "2026-07-06T09:12:00",
        note: "Request raised by Priya Sharma",
      },
      {
        id: "t2",
        status: "Assigned",
        changedBy: "Divya Nair",
        changedAt: "2026-07-06T09:45:00",
        note: "Assigned to technician Ronak",
      },
      {
        id: "t3",
        status: "In Progress",
        changedBy: "Ronak",
        changedAt: "2026-07-06T10:05:00",
        note: "Status updated to In Progress",
      },
    ],
    attachments: [
      {
        id: "a1",
        name: "screenshot_error.png",
        size: "245 KB",
        url: "#",
      },
      {
        id: "a2",
        name: "diagnostic_log.txt",
        size: "14 KB",
        url: "#",
      },
    ],
  },
  {
    id: "2",
    no: "SR-2026-1040",
    title: "AC not cooling in Conference Room B",
    description:
      "The air conditioning in Conference Room B has stopped cooling. Room becomes unusable by mid-day.",
    serviceType: "Facility",
    requestType: "AC Repair",
    department: "Maintenance",
    requester: "Karan Patel",
    requesterEmail: "hod@gmail.com",
    assignee: "Suresh Kumar",
    status: "Pending",
    priority: "High",
    created: "2026-07-06T08:40:00",
    updated: "2026-07-06T08:40:00",
    replies: [],
    timeline: [
      {
        id: "t1",
        status: "Pending",
        changedBy: "Karan Patel",
        changedAt: "2026-07-06T08:40:00",
        note: "Request raised by HOD Karan Patel",
      },
    ],
    attachments: [
      {
        id: "a3",
        name: "conference_room_temp.png",
        size: "180 KB",
        url: "#",
      },
    ],
  },
  {
    id: "3",
    no: "SR-2026-1039",
    title: "Request for new software license — Figma",
    description:
      "Design team needs 3 additional Figma professional seats for the new product squad.",
    serviceType: "Technical",
    requestType: "Software Request",
    department: "IT",
    requester: "Neha Gupta",
    requesterEmail: "requestor@gmail.com",
    assignee: null,
    status: "Pending",
    priority: "Medium",
    created: "2026-07-05T16:20:00",
    updated: "2026-07-05T16:20:00",
    replies: [],
    timeline: [
      {
        id: "t1",
        status: "Pending",
        changedBy: "Neha Gupta",
        changedAt: "2026-07-05T16:20:00",
        note: "Request raised and pending approval",
      },
    ],
  },
  {
    id: "4",
    no: "SR-2026-1038",
    title: "Projector flickering in Training Hall",
    description: "The ceiling projector flickers intermittently during presentations.",
    serviceType: "Facility",
    requestType: "AV Equipment",
    department: "Maintenance",
    requester: "Amit Singh",
    requesterEmail: "requestor@gmail.com",
    assignee: "Suresh Kumar",
    status: "Completed",
    priority: "Medium",
    created: "2026-07-04T11:00:00",
    updated: "2026-07-06T15:45:00",
    replies: [
      {
        id: 1,
        author: "Suresh Kumar",
        role: "Technician",
        message: "Replaced the HDMI cable and cleaned the lens. Please verify.",
        date: "2026-07-06T15:45:00",
        status: "Completed",
      },
    ],
    timeline: [
      {
        id: "t1",
        status: "Pending",
        changedBy: "Amit Singh",
        changedAt: "2026-07-04T11:00:00",
        note: "Request raised by Amit Singh",
      },
      {
        id: "t2",
        status: "Assigned",
        changedBy: "Karan Patel",
        changedAt: "2026-07-04T13:00:00",
        note: "Assigned to technician Suresh Kumar",
      },
      {
        id: "t3",
        status: "In Progress",
        changedBy: "Suresh Kumar",
        changedAt: "2026-07-05T09:30:00",
        note: "Suresh Kumar started work",
      },
      {
        id: "t4",
        status: "Completed",
        changedBy: "Suresh Kumar",
        changedAt: "2026-07-06T15:45:00",
        note: "Status updated to Completed",
      },
    ],
  },
  {
    id: "5",
    no: "SR-2026-1037",
    title: "VPN access for remote contractor",
    description:
      "Need VPN credentials provisioned for contractor working on the data migration project (3 months).",
    serviceType: "Technical",
    requestType: "Access Request",
    department: "IT",
    requester: "Divya Nair",
    requesterEmail: "hod@gmail.com",
    assignee: "Ronak",
    status: "Pending",
    priority: "High",
    created: "2026-07-04T09:30:00",
    updated: "2026-07-05T10:00:00",
    replies: [],
    timeline: [
      {
        id: "t1",
        status: "Pending",
        changedBy: "Divya Nair",
        changedAt: "2026-07-04T09:30:00",
        note: "Request raised and awaiting approval",
      },
    ],
  },
  {
    id: "6",
    no: "SR-2026-1036",
    title: "Desk chair replacement — ergonomic issue",
    description:
      "Current chair is broken and causing back strain. Requesting ergonomic replacement.",
    serviceType: "Administrative",
    requestType: "Furniture",
    department: "Housekeeping",
    requester: "Vikram Rao",
    requesterEmail: "requestor@gmail.com",
    assignee: "Meena Joshi",
    status: "Closed",
    priority: "Low",
    created: "2026-07-02T13:15:00",
    updated: "2026-07-05T09:00:00",
    replies: [
      {
        id: 1,
        author: "Meena Joshi",
        role: "Technician",
        message: "New chair delivered to desk 4B-12.",
        date: "2026-07-04T16:00:00",
        status: "Completed",
      },
      {
        id: 2,
        author: "Vikram Rao",
        role: "Requestor",
        message: "Received, thank you!",
        date: "2026-07-05T09:00:00",
      },
    ],
    timeline: [
      {
        id: "t1",
        status: "Pending",
        changedBy: "Vikram Rao",
        changedAt: "2026-07-02T13:15:00",
        note: "Request raised by Vikram Rao",
      },
      {
        id: "t2",
        status: "Assigned",
        changedBy: "Admin",
        changedAt: "2026-07-03T10:00:00",
        note: "Assigned to technician Meena Joshi",
      },
      {
        id: "t3",
        status: "Completed",
        changedBy: "Meena Joshi",
        changedAt: "2026-07-04T16:00:00",
        note: "Status updated to Completed",
      },
      {
        id: "t4",
        status: "Closed",
        changedBy: "Vikram Rao",
        changedAt: "2026-07-05T09:00:00",
        note: "Status updated to Closed",
      },
    ],
  },
  {
    id: "7",
    no: "SR-2026-1035",
    title: "Email delivery delays to external domains",
    description: "Outbound emails to client domains are delayed by 30+ minutes since Monday.",
    serviceType: "Technical",
    requestType: "Email Issue",
    department: "IT",
    requester: "Sanjay Iyer",
    requesterEmail: "requestor@gmail.com",
    assignee: "Anita Desai",
    status: "In Progress",
    priority: "Critical",
    created: "2026-07-03T10:45:00",
    updated: "2026-07-07T11:20:00",
    replies: [
      {
        id: 1,
        author: "Anita Desai",
        role: "Technician",
        message: "Investigating mail queue backlog on the relay server.",
        date: "2026-07-03T12:00:00",
        status: "In Progress",
      },
    ],
    timeline: [
      {
        id: "t1",
        status: "Pending",
        changedBy: "Sanjay Iyer",
        changedAt: "2026-07-03T10:45:00",
        note: "Request raised by Sanjay Iyer",
      },
      {
        id: "t2",
        status: "Assigned",
        changedBy: "Divya Nair",
        changedAt: "2026-07-03T11:30:00",
        note: "Assigned to technician Anita Desai",
      },
      {
        id: "t3",
        status: "In Progress",
        changedBy: "Anita Desai",
        changedAt: "2026-07-03T12:00:00",
        note: "Status updated to In Progress",
      },
    ],
  },
  {
    id: "8",
    no: "SR-2026-1034",
    title: "Water dispenser leaking on Floor 3",
    description: "The dispenser near the east wing pantry is leaking and creating a slip hazard.",
    serviceType: "Facility",
    requestType: "Plumbing",
    department: "Housekeeping",
    requester: "Ritu Malhotra",
    requesterEmail: "requestor@gmail.com",
    assignee: "Meena Joshi",
    status: "Completed",
    priority: "High",
    created: "2026-07-03T08:00:00",
    updated: "2026-07-04T12:30:00",
    replies: [],
    timeline: [
      {
        id: "t1",
        status: "Pending",
        changedBy: "Ritu Malhotra",
        changedAt: "2026-07-03T08:00:00",
        note: "Request raised by Ritu Malhotra",
      },
      {
        id: "t2",
        status: "Completed",
        changedBy: "Meena Joshi",
        changedAt: "2026-07-04T12:30:00",
        note: "Dispenser valve replaced. Status updated to Completed",
      },
    ],
  },
  {
    id: "9",
    no: "SR-2026-1033",
    title: "Monitor upgrade for design workstation",
    description: "Requesting a 27-inch 4K monitor for color-accurate design work.",
    serviceType: "Technical",
    requestType: "Hardware Request",
    department: "IT",
    requester: "Neha Gupta",
    requesterEmail: "requestor@gmail.com",
    assignee: null,
    status: "Pending",
    priority: "Low",
    created: "2026-07-02T15:30:00",
    updated: "2026-07-02T15:30:00",
    replies: [],
    timeline: [
      {
        id: "t1",
        status: "Pending",
        changedBy: "Neha Gupta",
        changedAt: "2026-07-02T15:30:00",
        note: "Request raised and awaiting HOD approval",
      },
    ],
  },
  {
    id: "10",
    no: "SR-2026-1032",
    title: "Printer offline in Finance wing",
    description: "Shared printer FIN-PR-02 shows offline for all users since this morning.",
    serviceType: "Technical",
    requestType: "Printer Issue",
    department: "IT",
    requester: "Rajesh Khanna",
    requesterEmail: "requestor@gmail.com",
    assignee: "Ronak",
    status: "Closed",
    priority: "Medium",
    created: "2026-07-01T09:20:00",
    updated: "2026-07-02T10:15:00",
    replies: [],
    timeline: [
      {
        id: "t1",
        status: "Pending",
        changedBy: "Rajesh Khanna",
        changedAt: "2026-07-01T09:20:00",
        note: "Request raised by Rajesh Khanna",
      },
      {
        id: "t2",
        status: "Completed",
        changedBy: "Ronak",
        changedAt: "2026-07-02T09:45:00",
        note: "Driver reinstalled. Status updated to Completed",
      },
      {
        id: "t3",
        status: "Closed",
        changedBy: "Rajesh Khanna",
        changedAt: "2026-07-02T10:15:00",
        note: "Ticket closed by requester",
      },
    ],
  },
  {
    id: "11",
    no: "SR-2026-1031",
    title: "ID card reader malfunction at main entrance",
    description: "Card reader intermittently rejects valid access cards at the main gate.",
    serviceType: "Facility",
    requestType: "Security Systems",
    department: "Maintenance",
    requester: "Pooja Reddy",
    requesterEmail: "requestor@gmail.com",
    assignee: "Suresh Kumar",
    status: "In Progress",
    priority: "High",
    created: "2026-06-30T14:10:00",
    updated: "2026-07-06T09:00:00",
    replies: [],
    timeline: [
      {
        id: "t1",
        status: "Pending",
        changedBy: "Pooja Reddy",
        changedAt: "2026-06-30T14:10:00",
        note: "Request raised by Pooja Reddy",
      },
      {
        id: "t2",
        status: "Assigned",
        changedBy: "Karan Patel",
        changedAt: "2026-07-01T08:00:00",
        note: "Assigned to Suresh Kumar",
      },
      {
        id: "t3",
        status: "In Progress",
        changedBy: "Suresh Kumar",
        changedAt: "2026-07-01T09:00:00",
        note: "Work started on gate scanner reader",
      },
    ],
  },
  {
    id: "12",
    no: "SR-2026-1030",
    title: "Onboarding setup for 4 new hires",
    description:
      "New engineering hires join July 14. Need laptops, accounts, and access badges provisioned.",
    serviceType: "Administrative",
    requestType: "Onboarding",
    department: "IT",
    requester: "HR Operations",
    requesterEmail: "requestor@gmail.com",
    assignee: "Ronak",
    status: "Assigned",
    priority: "Medium",
    created: "2026-06-30T11:00:00",
    updated: "2026-06-30T11:00:00",
    replies: [],
    timeline: [
      {
        id: "t1",
        status: "Pending",
        changedBy: "HR Operations",
        changedAt: "2026-06-30T11:00:00",
        note: "Request raised by HR Operations",
      },
      {
        id: "t2",
        status: "Assigned",
        changedBy: "Divya Nair",
        changedAt: "2026-07-01T10:00:00",
        note: "Assigned to Ronak",
      },
    ],
  },
  {
    id: "13",
    no: "SR-2026-1029",
    title: "Server room temperature alert",
    description:
      "Temperature in server room exceeded threshold twice this week. HVAC inspection needed.",
    serviceType: "Facility",
    requestType: "AC Repair",
    department: "Maintenance",
    requester: "Anita Desai",
    requesterEmail: "tech@gmail.com",
    assignee: "Suresh Kumar",
    status: "Completed",
    priority: "Critical",
    created: "2026-06-28T07:45:00",
    updated: "2026-07-01T17:00:00",
    replies: [],
    timeline: [
      {
        id: "t1",
        status: "Pending",
        changedBy: "Anita Desai",
        changedAt: "2026-06-28T07:45:00",
        note: "Request raised by Anita Desai",
      },
      {
        id: "t2",
        status: "Completed",
        changedBy: "Suresh Kumar",
        changedAt: "2026-07-01T17:00:00",
        note: "AC condenser coils cleaned. Status updated to Completed",
      },
    ],
  },
  {
    id: "14",
    no: "SR-2026-1028",
    title: "Carpet cleaning — Floor 2 west wing",
    description: "Scheduled deep cleaning request for the west wing carpets.",
    serviceType: "Facility",
    requestType: "Cleaning",
    department: "Housekeeping",
    requester: "Admin Office",
    requesterEmail: "admin@gmail.com",
    assignee: "Meena Joshi",
    status: "Closed",
    priority: "Low",
    created: "2026-06-27T10:00:00",
    updated: "2026-06-29T18:00:00",
    replies: [],
    timeline: [
      {
        id: "t1",
        status: "Pending",
        changedBy: "Admin Office",
        changedAt: "2026-06-27T10:00:00",
        note: "Request raised by Admin Office",
      },
      {
        id: "t2",
        status: "Completed",
        changedBy: "Meena Joshi",
        changedAt: "2026-06-28T18:00:00",
        note: "Cleaning finished. Status updated to Completed",
      },
      {
        id: "t3",
        status: "Closed",
        changedBy: "Admin Office",
        changedAt: "2026-06-29T18:00:00",
        note: "Status updated to Closed",
      },
    ],
  },
  {
    id: "15",
    no: "SR-2026-1027",
    title: "Slow Wi-Fi in cafeteria area",
    description: "Wi-Fi speeds drop significantly during lunch hours in the cafeteria.",
    serviceType: "Technical",
    requestType: "Network Issue",
    department: "IT",
    requester: "Multiple Users",
    requesterEmail: "requestor@gmail.com",
    assignee: "Ronak",
    status: "In Progress",
    priority: "Medium",
    created: "2026-06-26T13:30:00",
    updated: "2026-07-03T16:20:00",
    replies: [],
    timeline: [
      {
        id: "t1",
        status: "Pending",
        changedBy: "Multiple Users",
        changedAt: "2026-06-26T13:30:00",
        note: "Request raised by Multiple Users",
      },
      {
        id: "t2",
        status: "Assigned",
        changedBy: "Divya Nair",
        changedAt: "2026-06-27T10:00:00",
        note: "Assigned to Ronak",
      },
      {
        id: "t3",
        status: "In Progress",
        changedBy: "Ronak",
        changedAt: "2026-06-28T14:00:00",
        note: "Status updated to In Progress",
      },
    ],
  },
];

const initialUsers = [
  {
    id: "1",
    name: "Bhaviik Parmar",
    email: "admin@gmail.com",
    password: "123456",
    role: "Admin",
    department: "IT Services",
    phone: "+91 6353712057",
    status: "Active",
    joined: "2021-03-15",
    requestsRaised: 12,
    requestsResolved: 0,
  },
  {
    id: "2",
    name: "Ronak",
    email: "tech@gmail.com",
    password: "123456",
    role: "Technician",
    department: "IT",
    phone: "+91 98200 22334",
    status: "Active",
    joined: "2022-01-10",
    requestsRaised: 4,
    requestsResolved: 148,
  },
  {
    id: "3",
    name: "Anita Desai",
    email: "anita.desai@company.com",
    password: "123456",
    role: "Technician",
    department: "IT",
    phone: "+91 98200 33445",
    status: "Active",
    joined: "2021-08-22",
    requestsRaised: 7,
    requestsResolved: 132,
  },
  {
    id: "4",
    name: "Suresh Kumar",
    email: "suresh.kumar@company.com",
    password: "123456",
    role: "Technician",
    department: "Maintenance",
    phone: "+91 98200 44556",
    status: "Active",
    joined: "2020-11-05",
    requestsRaised: 2,
    requestsResolved: 205,
  },
  {
    id: "5",
    name: "Meena Joshi",
    email: "meena.joshi@company.com",
    password: "123456",
    role: "Technician",
    department: "Housekeeping",
    phone: "+91 98200 55667",
    status: "Active",
    joined: "2022-06-18",
    requestsRaised: 1,
    requestsResolved: 96,
  },
  {
    id: "6",
    name: "Priya Sharma",
    email: "requestor@gmail.com",
    password: "123456",
    role: "Requestor",
    department: "Sales",
    phone: "+91 98200 66778",
    status: "Active",
    joined: "2023-02-01",
    requestsRaised: 23,
    requestsResolved: 0,
  },
  {
    id: "7",
    name: "Karan Patel",
    email: "hod@gmail.com",
    password: "123456",
    role: "HOD",
    department: "Operations",
    phone: "+91 98200 77889",
    status: "Active",
    joined: "2019-07-12",
    requestsRaised: 9,
    requestsResolved: 0,
  },
  {
    id: "8",
    name: "Neha Gupta",
    email: "neha.gupta@company.com",
    password: "123456",
    role: "Requestor",
    department: "Design",
    phone: "+91 98200 88990",
    status: "Active",
    joined: "2023-09-04",
    requestsRaised: 17,
    requestsResolved: 0,
  },
  {
    id: "9",
    name: "Vikram Rao",
    email: "vikram.rao@company.com",
    password: "123456",
    role: "Requestor",
    department: "Finance",
    phone: "+91 98200 99001",
    status: "Inactive",
    joined: "2020-04-20",
    requestsRaised: 31,
    requestsResolved: 0,
  },
  {
    id: "10",
    name: "Divya Nair",
    email: "divya.nair@company.com",
    password: "123456",
    role: "HOD",
    department: "IT",
    phone: "+91 98200 10112",
    status: "Active",
    joined: "2018-12-01",
    requestsRaised: 6,
    requestsResolved: 0,
  },
];

const initialAssets = [
  {
    id: "1",
    tag: "AST-IT-0142",
    name: "Dell Latitude 7440",
    category: "Laptop",
    serial: "DL7440-98213",
    assignedTo: "Priya Sharma",
    department: "Sales",
    status: "In Use",
    purchaseDate: "2024-05-12",
    warrantyUntil: "2027-05-12",
    value: "₹98,000",
  },
  {
    id: "2",
    tag: "AST-IT-0143",
    name: "MacBook Pro 14 M3",
    category: "Laptop",
    serial: "MBP14-55120",
    assignedTo: "Neha Gupta",
    department: "Design",
    status: "In Use",
    purchaseDate: "2024-08-01",
    warrantyUntil: "2027-08-01",
    value: "₹1,85,000",
  },
  {
    id: "3",
    tag: "AST-IT-0098",
    name: "HP LaserJet Pro M428",
    category: "Printer",
    serial: "HPLJ-77341",
    assignedTo: null,
    department: "Finance",
    status: "Under Repair",
    purchaseDate: "2022-03-20",
    warrantyUntil: "2025-03-20",
    value: "₹42,000",
  },
  {
    id: "4",
    tag: "AST-IT-0201",
    name: "Dell UltraSharp 27 4K",
    category: "Monitor",
    serial: "DU27-11298",
    assignedTo: null,
    department: "IT",
    status: "Available",
    purchaseDate: "2025-01-15",
    warrantyUntil: "2028-01-15",
    value: "₹52,000",
  },
  {
    id: "5",
    tag: "AST-FC-0034",
    name: "Daikin 2T Split AC",
    category: "HVAC",
    serial: "DK2T-88123",
    assignedTo: null,
    department: "Maintenance",
    status: "Under Repair",
    purchaseDate: "2021-06-10",
    warrantyUntil: "2024-06-10",
    value: "₹65,000",
  },
  {
    id: "6",
    tag: "AST-IT-0177",
    name: "Lenovo ThinkPad X1",
    category: "Laptop",
    serial: "LTX1-33409",
    assignedTo: "Sanjay Iyer",
    department: "Marketing",
    status: "In Use",
    purchaseDate: "2023-11-25",
    warrantyUntil: "2026-11-25",
    value: "₹1,25,000",
  },
  {
    id: "7",
    tag: "AST-IT-0055",
    name: "Cisco Catalyst 9200 Switch",
    category: "Network",
    serial: "CC92-20981",
    assignedTo: null,
    department: "IT",
    status: "In Use",
    purchaseDate: "2022-09-08",
    warrantyUntil: "2027-09-08",
    value: "₹2,40,000",
  },
  {
    id: "8",
    tag: "AST-AV-0012",
    name: "Epson EB-2250U Projector",
    category: "AV Equipment",
    serial: "EP22-45671",
    assignedTo: null,
    department: "Admin",
    status: "In Use",
    purchaseDate: "2021-02-14",
    warrantyUntil: "2024-02-14",
    value: "₹88,000",
  },
  {
    id: "9",
    tag: "AST-IT-0089",
    name: "iPad Pro 12.9",
    category: "Tablet",
    serial: "IPP12-90341",
    assignedTo: "Karan Patel",
    department: "Operations",
    status: "In Use",
    purchaseDate: "2023-04-30",
    warrantyUntil: "2025-04-30",
    value: "₹1,12,000",
  },
  {
    id: "10",
    tag: "AST-IT-0021",
    name: "HP EliteDesk 800 G5",
    category: "Desktop",
    serial: "HPED-11276",
    assignedTo: null,
    department: "Finance",
    status: "Retired",
    purchaseDate: "2019-08-19",
    warrantyUntil: "2022-08-19",
    value: "₹68,000",
  },
];

const initialNotifications = [
  {
    id: 1,
    title: "New request assigned",
    message: "SR-2026-1041 'Laptop not booting' has been assigned to Rohan Verma.",
    time: "10 min ago",
    read: false,
    type: "request",
  },
  {
    id: 2,
    title: "Approval pending",
    message: "SR-2026-1039 'Figma licenses' is awaiting your approval.",
    time: "42 min ago",
    read: false,
    type: "approval",
  },
  {
    id: 3,
    title: "Request resolved",
    message: "SR-2026-1038 'Projector flickering' was marked resolved by Suresh Kumar.",
    time: "2 hours ago",
    read: false,
    type: "request",
  },
  {
    id: 4,
    title: "Asset returned",
    message: "Dell UltraSharp 27 4K monitor returned to inventory.",
    time: "5 hours ago",
    read: true,
    type: "asset",
  },
  {
    id: 5,
    title: "SLA warning",
    message: "SR-2026-1035 'Email delays' is approaching its SLA deadline.",
    time: "Yesterday",
    read: true,
    type: "system",
  },
  {
    id: 6,
    title: "Approval granted",
    message: "VPN access request SR-2026-1037 was approved by Divya Nair.",
    time: "Yesterday",
    read: true,
    type: "approval",
  },
  {
    id: 7,
    title: "System maintenance",
    message: "Scheduled maintenance window on Sunday 2 AM – 4 AM IST.",
    time: "2 days ago",
    read: true,
    type: "system",
  },
];

const initialApprovals = [
  {
    id: "1",
    requestId: "3",
    requestNo: "SR-2026-1039",
    title: "Request for new software license — Figma",
    requester: "Neha Gupta",
    department: "Design",
    priority: "Medium",
    submitted: "2026-07-05T16:20:00",
    status: "Pending",
  },
  {
    id: "2",
    requestId: "5",
    requestNo: "SR-2026-1037",
    title: "VPN access for remote contractor",
    requester: "Divya Nair",
    department: "IT",
    priority: "High",
    submitted: "2026-07-04T09:30:00",
    status: "Pending",
  },
  {
    id: "3",
    requestId: "12",
    requestNo: "SR-2026-1030",
    title: "Onboarding setup for 4 new hires",
    requester: "HR Operations",
    department: "IT",
    priority: "Medium",
    submitted: "2026-06-30T11:00:00",
    status: "Approved",
    decidedBy: "Divya Nair",
    decidedOn: "2026-07-01T10:15:00",
    remarks: "Approved. Coordinate with HR for badge photos.",
  },
  {
    id: "4",
    requestNo: "SR-2026-1026",
    requestId: "9",
    title: "Standing desk for development team (x6)",
    requester: "Sanjay Iyer",
    department: "Engineering",
    priority: "Low",
    submitted: "2026-06-25T14:00:00",
    status: "Rejected",
    decidedBy: "Karan Patel",
    decidedOn: "2026-06-26T09:30:00",
    remarks: "Deferred to next quarter's budget cycle.",
  },
  {
    id: "5",
    requestNo: "SR-2026-1024",
    requestId: "13",
    title: "HVAC inspection contract renewal",
    requester: "Anita Desai",
    department: "Maintenance",
    priority: "Critical",
    submitted: "2026-06-24T08:45:00",
    status: "Approved",
    decidedBy: "Karan Patel",
    decidedOn: "2026-06-24T11:00:00",
    remarks: "Critical infrastructure — fast-tracked.",
  },
];

// Master seed data
const initialStatuses = [
  {
    id: "1",
    name: "Open",
    color: "bg-info/10 text-info ring-info/20",
    description: "Request is raised and awaiting assignment or review",
    isActive: true,
  },
  {
    id: "2",
    name: "In Progress",
    color: "bg-warning/15 text-warning-foreground ring-warning/30 dark:text-warning",
    description: "Technician is actively working on the request",
    isActive: true,
  },
  {
    id: "3",
    name: "Pending Approval",
    color: "bg-primary/10 text-primary ring-primary/20",
    description: "Awaiting HOD or manager sign-off",
    isActive: true,
  },
  {
    id: "4",
    name: "Resolved",
    color: "bg-success/10 text-success ring-success/20",
    description: "Fix is applied and awaiting requestor closure",
    isActive: true,
  },
  {
    id: "5",
    name: "Closed",
    color: "bg-muted text-muted-foreground ring-border",
    description: "Request has been completed and closed",
    isActive: true,
  },
];

const initialDepartments = [
  {
    id: "1",
    name: "IT",
    code: "IT",
    description: "Information Technology support and resources",
    isActive: true,
  },
  {
    id: "2",
    name: "Maintenance",
    code: "MAINT",
    description: "Facilities upkeep and physical repairs",
    isActive: true,
  },
  {
    id: "3",
    name: "Housekeeping",
    code: "HOUSE",
    description: "Cleaning, sanitation and office furniture services",
    isActive: true,
  },
  {
    id: "4",
    name: "Admin",
    code: "ADM",
    description: "Office administrative tasks, procurement and onboarding",
    isActive: true,
  },
  {
    id: "5",
    name: "Security",
    code: "SEC",
    description: "Badges, access cards and physical security systems",
    isActive: true,
  },
];

const initialServiceTypes = [
  {
    id: "1",
    name: "Technical",
    code: "TECH",
    description: "Software, hardware, network and account support",
    isActive: true,
  },
  {
    id: "2",
    name: "Facility",
    code: "FAC",
    description: "Building, AC, plumbing and physical maintenance",
    isActive: true,
  },
  {
    id: "3",
    name: "Administrative",
    code: "ADMIN",
    description: "Office operations, furniture, badges and onboarding support",
    isActive: true,
  },
];

const initialRequestTypes = [
  {
    id: "1",
    name: "Computer Issue",
    serviceTypeId: "1",
    description: "Laptops, desktops, operating systems and boots",
    isActive: true,
  },
  {
    id: "2",
    name: "Software Request",
    serviceTypeId: "1",
    description: "Figma, Adobe, IDEs or other license requests",
    isActive: true,
  },
  {
    id: "3",
    name: "Hardware Request",
    serviceTypeId: "1",
    description: "Monitors, keyboards, RAM upgrades or mouse replacements",
    isActive: true,
  },
  {
    id: "4",
    name: "Network Issue",
    serviceTypeId: "1",
    description: "Wi-Fi speeds, ethernet connections or office routers",
    isActive: true,
  },
  {
    id: "5",
    name: "Email Issue",
    serviceTypeId: "1",
    description: "Email delivery, Outlook, Thunderbird or Gmail configurations",
    isActive: true,
  },
  {
    id: "6",
    name: "Printer Issue",
    serviceTypeId: "1",
    description: "Shared office printer offline, toner or paper jams",
    isActive: true,
  },
  {
    id: "7",
    name: "Access Request",
    serviceTypeId: "1",
    description: "VPN keys, Github access or server credentials",
    isActive: true,
  },
  {
    id: "8",
    name: "AC Repair",
    serviceTypeId: "2",
    description: "Cooling system issues, leaks or remote control issues",
    isActive: true,
  },
  {
    id: "9",
    name: "Plumbing",
    serviceTypeId: "2",
    description: "Water leaks, washroom or pantry tap repairs",
    isActive: true,
  },
  {
    id: "10",
    name: "AV Equipment",
    serviceTypeId: "2",
    description: "Projectors, TV screens, microphones and speakers in conference rooms",
    isActive: true,
  },
  {
    id: "11",
    name: "Security Systems",
    serviceTypeId: "2",
    description: "Malfunctioning gate card readers, CCTV or locks",
    isActive: true,
  },
  {
    id: "12",
    name: "Cleaning",
    serviceTypeId: "2",
    description: "Deep cleaning, vacuuming, waste management or spills",
    isActive: true,
  },
  {
    id: "13",
    name: "Furniture",
    serviceTypeId: "3",
    description: "Ergonomic chair replacement, drawer keys or desk setup",
    isActive: true,
  },
  {
    id: "14",
    name: "Onboarding",
    serviceTypeId: "3",
    description: "Setting up workspace and accounts for new hires",
    isActive: true,
  },
];

const initialDepartmentPersons = [
  { id: "1", userId: "2", departmentId: "1", isHOD: false, isActive: true }, // Ronak (Tech, IT)
  { id: "2", userId: "3", departmentId: "1", isHOD: false, isActive: true }, // Anita Desai (Tech, IT)
  { id: "3", userId: "4", departmentId: "2", isHOD: false, isActive: true }, // Suresh Kumar (Tech, Maintenance)
  { id: "4", userId: "5", departmentId: "3", isHOD: false, isActive: true }, // Meena Joshi (Tech, Housekeeping)
  { id: "5", userId: "7", departmentId: "2", isHOD: true, isActive: true }, // Karan Patel (HOD, Maintenance)
  { id: "6", userId: "10", departmentId: "1", isHOD: true, isActive: true }, // Divya Nair (HOD, IT)
];

const initialRequestTypePersonMappings = [
  { id: "1", requestTypeId: "1", personId: "1", isActive: true }, // Computer Issue -> Ronak
  { id: "2", requestTypeId: "2", personId: "2", isActive: true }, // Software Request -> Anita Desai
  { id: "3", requestTypeId: "3", personId: "1", isActive: true }, // Hardware Request -> Ronak
  { id: "4", requestTypeId: "4", personId: "1", isActive: true }, // Network Issue -> Ronak
  { id: "5", requestTypeId: "5", personId: "2", isActive: true }, // Email Issue -> Anita Desai
  { id: "6", requestTypeId: "6", personId: "1", isActive: true }, // Printer Issue -> Ronak
  { id: "7", requestTypeId: "7", personId: "2", isActive: true }, // Access Request -> Anita Desai
  { id: "8", requestTypeId: "8", personId: "3", isActive: true }, // AC Repair -> Suresh Kumar
  { id: "9", requestTypeId: "9", personId: "3", isActive: true }, // Plumbing -> Suresh Kumar
  { id: "10", requestTypeId: "10", personId: "3", isActive: true }, // AV Equipment -> Suresh Kumar
  { id: "11", requestTypeId: "11", personId: "3", isActive: true }, // Security Systems -> Suresh Kumar
  { id: "12", requestTypeId: "12", personId: "4", isActive: true }, // Cleaning -> Meena Joshi
  { id: "13", requestTypeId: "13", personId: "4", isActive: true }, // Furniture -> Meena Joshi
  { id: "14", requestTypeId: "14", personId: "2", isActive: true }, // Onboarding -> Anita Desai
];

// Live collections synced with localStorage (mutated in-place)
export const requests = [];
export const users = [];
export const assets = [];
export const notifications = [];
export const approvals = [];

export const statuses = [];
export const departmentsMaster = [];
export const departmentPersons = [];
export const serviceTypesMaster = [];
export const requestTypesMaster = [];
export const requestTypePersonMappings = [];

export const serviceTypes = [];
export const requestTypes = [];
export const departments = [];
export const technicians = [];

// Fallback current user (though components should prefer useAuth profile)
export const currentUser = {
  name: "Aarav Sharma",
  email: "admin@company.com",
  role: "Admin",
  department: "IT Services",
  avatar: "AS",
};

export function syncLocalStorage() {
  if (typeof window === "undefined") return;

  // Sync requests
  try {
    const data = localStorage.getItem("servicedesk.requests");
    requests.length = 0;
    if (data) {
      const parsed = JSON.parse(data);
      const needsMigration = parsed.length > 0 && !parsed[0].timeline;
      if (needsMigration) {
        localStorage.setItem("servicedesk.requests", JSON.stringify(initialRequests));
        requests.push(...initialRequests);
      } else {
        requests.push(...parsed);
      }
    } else {
      localStorage.setItem("servicedesk.requests", JSON.stringify(initialRequests));
      requests.push(...initialRequests);
    }
  } catch (e) {
    console.error("Failed to sync requests", e);
  }

  // Sync users
  try {
    const data = localStorage.getItem("servicedesk.users");
    users.length = 0;
    if (data) {
      users.push(...JSON.parse(data));
    } else {
      localStorage.setItem("servicedesk.users", JSON.stringify(initialUsers));
      users.push(...initialUsers);
    }
  } catch (e) {
    console.error("Failed to sync users", e);
  }

  // Sync assets
  try {
    const data = localStorage.getItem("servicedesk.assets");
    assets.length = 0;
    if (data) {
      assets.push(...JSON.parse(data));
    } else {
      localStorage.setItem("servicedesk.assets", JSON.stringify(initialAssets));
      assets.push(...initialAssets);
    }
  } catch (e) {
    console.error("Failed to sync assets", e);
  }

  // Sync notifications
  try {
    const data = localStorage.getItem("servicedesk.notifications");
    notifications.length = 0;
    if (data) {
      notifications.push(...JSON.parse(data));
    } else {
      localStorage.setItem("servicedesk.notifications", JSON.stringify(initialNotifications));
      notifications.push(...initialNotifications);
    }
  } catch (e) {
    console.error("Failed to sync notifications", e);
  }

  // Sync approvals
  try {
    const data = localStorage.getItem("servicedesk.approvals");
    approvals.length = 0;
    if (data) {
      approvals.push(...JSON.parse(data));
    } else {
      localStorage.setItem("servicedesk.approvals", JSON.stringify(initialApprovals));
      approvals.push(...initialApprovals);
    }
  } catch (e) {
    console.error("Failed to sync approvals", e);
  }

  // Sync Statuses
  try {
    const data = localStorage.getItem("servicedesk.statuses");
    statuses.length = 0;
    if (data) {
      statuses.push(...JSON.parse(data));
    } else {
      localStorage.setItem("servicedesk.statuses", JSON.stringify(initialStatuses));
      statuses.push(...initialStatuses);
    }
  } catch (e) {
    console.error("Failed to sync statuses", e);
  }

  // Sync Departments Master
  try {
    const data = localStorage.getItem("servicedesk.departments_master");
    departmentsMaster.length = 0;
    if (data) {
      departmentsMaster.push(...JSON.parse(data));
    } else {
      localStorage.setItem("servicedesk.departments_master", JSON.stringify(initialDepartments));
      departmentsMaster.push(...initialDepartments);
    }
  } catch (e) {
    console.error("Failed to sync departments master", e);
  }

  // Sync Department Persons Master
  try {
    const data = localStorage.getItem("servicedesk.department_persons");
    departmentPersons.length = 0;
    if (data) {
      departmentPersons.push(...JSON.parse(data));
    } else {
      localStorage.setItem(
        "servicedesk.department_persons",
        JSON.stringify(initialDepartmentPersons),
      );
      departmentPersons.push(...initialDepartmentPersons);
    }
  } catch (e) {
    console.error("Failed to sync department persons", e);
  }

  // Sync Service Types
  try {
    const data = localStorage.getItem("servicedesk.service_types");
    serviceTypesMaster.length = 0;
    if (data) {
      serviceTypesMaster.push(...JSON.parse(data));
    } else {
      localStorage.setItem("servicedesk.service_types", JSON.stringify(initialServiceTypes));
      serviceTypesMaster.push(...initialServiceTypes);
    }
  } catch (e) {
    console.error("Failed to sync service types master", e);
  }

  // Sync Request Types
  try {
    const data = localStorage.getItem("servicedesk.request_types");
    requestTypesMaster.length = 0;
    if (data) {
      requestTypesMaster.push(...JSON.parse(data));
    } else {
      localStorage.setItem("servicedesk.request_types", JSON.stringify(initialRequestTypes));
      requestTypesMaster.push(...initialRequestTypes);
    }
  } catch (e) {
    console.error("Failed to sync request types master", e);
  }

  // Sync Request Type Person Mappings
  try {
    const data = localStorage.getItem("servicedesk.request_type_person_mappings");
    requestTypePersonMappings.length = 0;
    if (data) {
      requestTypePersonMappings.push(...JSON.parse(data));
    } else {
      localStorage.setItem(
        "servicedesk.request_type_person_mappings",
        JSON.stringify(initialRequestTypePersonMappings),
      );
      requestTypePersonMappings.push(...initialRequestTypePersonMappings);
    }
  } catch (e) {
    console.error("Failed to sync request type person mappings", e);
  }

  // Sync the core static lists
  try {
    // Sync departments
    departments.length = 0;
    departments.push(...departmentsMaster.filter((d) => d.isActive).map((d) => d.name));

    // Sync serviceTypes
    serviceTypes.length = 0;
    serviceTypes.push(...serviceTypesMaster.filter((st) => st.isActive).map((st) => st.name));

    // Sync requestTypes
    requestTypes.length = 0;
    requestTypes.push(...requestTypesMaster.filter((rt) => rt.isActive).map((rt) => rt.name));

    // Sync technicians list
    technicians.length = 0;
    const techUserIds = new Set(
      departmentPersons.filter((dp) => dp.isActive).map((dp) => dp.userId),
    );
    const activeTechNames = users.filter((u) => techUserIds.has(u.id)).map((u) => u.name);
    technicians.push(...activeTechNames);
    if (technicians.length === 0) {
      technicians.push("Rohan Verma", "Anita Desai", "Suresh Kumar", "Meena Joshi");
    }
  } catch (e) {
    console.error("Failed to populate core lists from masters", e);
  }
}

// Perform initial sync
syncLocalStorage();

export function saveRequestsToStorage() {
  if (typeof window !== "undefined") {
    localStorage.setItem("servicedesk.requests", JSON.stringify(requests));
  }
}

export function saveUsersToStorage() {
  if (typeof window !== "undefined") {
    localStorage.setItem("servicedesk.users", JSON.stringify(users));
  }
}

export function saveNotificationsToStorage() {
  if (typeof window !== "undefined") {
    localStorage.setItem("servicedesk.notifications", JSON.stringify(notifications));
  }
}

export function saveApprovalsToStorage() {
  if (typeof window !== "undefined") {
    localStorage.setItem("servicedesk.approvals", JSON.stringify(approvals));
  }
}

export function saveStatusesToStorage() {
  if (typeof window !== "undefined") {
    localStorage.setItem("servicedesk.statuses", JSON.stringify(statuses));
  }
}

export function saveDepartmentsMasterToStorage() {
  if (typeof window !== "undefined") {
    localStorage.setItem("servicedesk.departments_master", JSON.stringify(departmentsMaster));
  }
}

export function saveDepartmentPersonsToStorage() {
  if (typeof window !== "undefined") {
    localStorage.setItem("servicedesk.department_persons", JSON.stringify(departmentPersons));
  }
}

export function saveServiceTypesMasterToStorage() {
  if (typeof window !== "undefined") {
    localStorage.setItem("servicedesk.service_types", JSON.stringify(serviceTypesMaster));
  }
}

export function saveRequestTypesMasterToStorage() {
  if (typeof window !== "undefined") {
    localStorage.setItem("servicedesk.request_types", JSON.stringify(requestTypesMaster));
  }
}

export function saveRequestTypePersonMappingsToStorage() {
  if (typeof window !== "undefined") {
    localStorage.setItem(
      "servicedesk.request_type_person_mappings",
      JSON.stringify(requestTypePersonMappings),
    );
  }
}

// CRUD actions
export function addRequest(req, changedBy = "System") {
  const timeline = req.timeline || [
    {
      id: String(Date.now()),
      status: req.status,
      changedBy: req.requester || changedBy,
      changedAt: new Date().toISOString(),
      note: `Request raised by ${req.requester}`,
    },
  ];

  const newReq = {
    ...req,
    timeline,
    created: req.created || new Date().toISOString(),
    updated: new Date().toISOString(),
  };

  requests.unshift(newReq);
  saveRequestsToStorage();
}

export function updateRequest(req, changedBy = "System", note = "") {
  const index = requests.findIndex((r) => r.id === req.id);
  if (index !== -1) {
    const oldReq = requests[index];

    const timeline = [...(req.timeline || oldReq.timeline || [])];

    if (oldReq.status !== req.status) {
      timeline.push({
        id: String(Date.now() + Math.random()),
        status: req.status,
        changedBy,
        changedAt: new Date().toISOString(),
        note: note || `Status updated from ${oldReq.status} to ${req.status}`,
      });
    }

    if (oldReq.assignee !== req.assignee) {
      timeline.push({
        id: String(Date.now() + Math.random()),
        status: req.status,
        changedBy,
        changedAt: new Date().toISOString(),
        note: req.assignee ? `Technician assigned: ${req.assignee}` : `Technician unassigned`,
      });
    }

    requests[index] = {
      ...req,
      timeline,
      updated: new Date().toISOString(),
    };
    saveRequestsToStorage();
  }
}

export function deleteRequest(id) {
  const index = requests.findIndex((r) => r.id === id);
  if (index !== -1) {
    requests.splice(index, 1);
    saveRequestsToStorage();
  }
}

export function addUser(user) {
  users.push(user);
  saveUsersToStorage();
  syncLocalStorage();
}

export function updateUser(user) {
  const index = users.findIndex((u) => u.id === user.id);
  if (index !== -1) {
    users[index] = user;
    saveUsersToStorage();
    syncLocalStorage();
  }
}

export function deleteUser(id) {
  const index = users.findIndex((u) => u.id === id);
  if (index !== -1) {
    users.splice(index, 1);
    saveUsersToStorage();
    syncLocalStorage();
  }
}

// ==========================================
// CONFIGURATION MASTERS CRUD HELPERS
// ==========================================

// 1. Service Request Status Master
export function addStatus(status) {
  statuses.push(status);
  saveStatusesToStorage();
  syncLocalStorage();
}

export function updateStatus(status) {
  const index = statuses.findIndex((s) => s.id === status.id);
  if (index !== -1) {
    statuses[index] = status;
    saveStatusesToStorage();
    syncLocalStorage();
  }
}

export function deleteStatus(id) {
  const index = statuses.findIndex((s) => s.id === id);
  if (index !== -1) {
    statuses.splice(index, 1);
    saveStatusesToStorage();
    syncLocalStorage();
  }
}

// 2. Service Department Master
export function addDepartment(dept) {
  departmentsMaster.push(dept);
  saveDepartmentsMasterToStorage();
  syncLocalStorage();
}

export function updateDepartment(dept) {
  const index = departmentsMaster.findIndex((d) => d.id === dept.id);
  if (index !== -1) {
    departmentsMaster[index] = dept;
    saveDepartmentsMasterToStorage();
    syncLocalStorage();
  }
}

export function deleteDepartment(id) {
  const index = departmentsMaster.findIndex((d) => d.id === id);
  if (index !== -1) {
    departmentsMaster.splice(index, 1);
    saveDepartmentsMasterToStorage();
    syncLocalStorage();
  }
}

// 3. Service Department Person Master
export function addDepartmentPerson(person) {
  departmentPersons.push(person);
  saveDepartmentPersonsToStorage();
  syncLocalStorage();
}

export function updateDepartmentPerson(person) {
  const index = departmentPersons.findIndex((dp) => dp.id === person.id);
  if (index !== -1) {
    departmentPersons[index] = person;
    saveDepartmentPersonsToStorage();
    syncLocalStorage();
  }
}

export function deleteDepartmentPerson(id) {
  const index = departmentPersons.findIndex((dp) => dp.id === id);
  if (index !== -1) {
    departmentPersons.splice(index, 1);
    saveDepartmentPersonsToStorage();
    syncLocalStorage();
  }
}

// 4. Service Type Master
export function addServiceType(st) {
  serviceTypesMaster.push(st);
  saveServiceTypesMasterToStorage();
  syncLocalStorage();
}

export function updateServiceType(st) {
  const index = serviceTypesMaster.findIndex((s) => s.id === st.id);
  if (index !== -1) {
    serviceTypesMaster[index] = st;
    saveServiceTypesMasterToStorage();
    syncLocalStorage();
  }
}

export function deleteServiceType(id) {
  const index = serviceTypesMaster.findIndex((s) => s.id === id);
  if (index !== -1) {
    serviceTypesMaster.splice(index, 1);
    saveServiceTypesMasterToStorage();
    syncLocalStorage();
  }
}

// 5. Service Request Type Master
export function addRequestType(rt) {
  requestTypesMaster.push(rt);
  saveRequestTypesMasterToStorage();
  syncLocalStorage();
}

export function updateRequestType(rt) {
  const index = requestTypesMaster.findIndex((r) => r.id === rt.id);
  if (index !== -1) {
    requestTypesMaster[index] = rt;
    saveRequestTypesMasterToStorage();
    syncLocalStorage();
  }
}

export function deleteRequestType(id) {
  const index = requestTypesMaster.findIndex((r) => r.id === id);
  if (index !== -1) {
    requestTypesMaster.splice(index, 1);
    saveRequestTypesMasterToStorage();
    syncLocalStorage();
  }
}

// 6. Service Request Type Wise Person Mapping
export function addRequestTypePersonMapping(mapping) {
  requestTypePersonMappings.push(mapping);
  saveRequestTypePersonMappingsToStorage();
  syncLocalStorage();
}

export function updateRequestTypePersonMapping(mapping) {
  const index = requestTypePersonMappings.findIndex((m) => m.id === mapping.id);
  if (index !== -1) {
    requestTypePersonMappings[index] = mapping;
    saveRequestTypePersonMappingsToStorage();
    syncLocalStorage();
  }
}

export function deleteRequestTypePersonMapping(id) {
  const index = requestTypePersonMappings.findIndex((m) => m.id === id);
  if (index !== -1) {
    requestTypePersonMappings.splice(index, 1);
    saveRequestTypePersonMappingsToStorage();
    syncLocalStorage();
  }
}

export function addNotification(notif) {
  const newNotif = {
    ...notif,
    id: Date.now(),
    read: false,
    time: "Just now",
  };
  notifications.unshift(newNotif);
  saveNotificationsToStorage();
}

export function markNotificationRead(id) {
  const notif = notifications.find((n) => n.id === id);
  if (notif) {
    notif.read = true;
    saveNotificationsToStorage();
  }
}

export function updateApproval(approval, decidedBy = "System", remarks = "") {
  const index = approvals.findIndex((a) => a.id === approval.id);
  if (index !== -1) {
    approvals[index] = approval;
    saveApprovalsToStorage();

    const req = requests.find((r) => r.id === approval.requestId);
    if (req) {
      const oldStatus = req.status;
      const newStatus = approval.status === "Approved" ? "Pending" : "Cancelled";

      const timeline = req.timeline || [];
      timeline.push({
        id: String(Date.now() + Math.random()),
        status: newStatus,
        changedBy: decidedBy,
        changedAt: new Date().toISOString(),
        note: `Approval decided: ${approval.status}. Remarks: ${remarks || "None"}.`,
      });

      const replies = req.replies || [];
      replies.push({
        id: Date.now(),
        author: decidedBy,
        role: "HOD",
        message: `HOD Decision: ${approval.status}.\nRemarks: ${remarks || "No remarks provided."}`,
        date: new Date().toISOString(),
      });

      req.status = newStatus;
      req.timeline = timeline;
      req.replies = replies;
      req.updated = new Date().toISOString();
      saveRequestsToStorage();
    }
  }
}

// Constants & Reports seed
export const monthlyRequests = [
  { month: "Aug", raised: 42, resolved: 38 },
  { month: "Sep", raised: 51, resolved: 47 },
  { month: "Oct", raised: 46, resolved: 44 },
  { month: "Nov", raised: 58, resolved: 52 },
  { month: "Dec", raised: 39, resolved: 41 },
  { month: "Jan", raised: 63, resolved: 55 },
  { month: "Feb", raised: 57, resolved: 59 },
  { month: "Mar", raised: 68, resolved: 62 },
  { month: "Apr", raised: 54, resolved: 56 },
  { month: "May", raised: 61, resolved: 58 },
  { month: "Jun", raised: 72, resolved: 66 },
  { month: "Jul", raised: 28, resolved: 19 },
];

export const statusDistribution = [
  { name: "Open", value: 3 },
  { name: "In Progress", value: 4 },
  { name: "Pending Approval", value: 2 },
  { name: "Resolved", value: 3 },
  { name: "Closed", value: 3 },
];

export const departmentReports = [
  { department: "IT", total: 248, resolved: 221, avgHours: 9.4 },
  { department: "Maintenance", total: 164, resolved: 152, avgHours: 14.2 },
  { department: "Housekeeping", total: 132, resolved: 128, avgHours: 6.8 },
  { department: "Admin", total: 76, resolved: 71, avgHours: 11.5 },
  { department: "Security", total: 44, resolved: 40, avgHours: 8.1 },
];

export const resolutionTrend = [
  { month: "Feb", hours: 14.2 },
  { month: "Mar", hours: 12.8 },
  { month: "Apr", hours: 11.5 },
  { month: "May", hours: 10.9 },
  { month: "Jun", hours: 9.6 },
  { month: "Jul", hours: 8.8 },
];

export const recentActivity = [
  {
    id: 1,
    actor: "Rohan Verma",
    action: "updated status of",
    target: "SR-2026-1041",
    detail: "to In Progress",
    time: "14 min ago",
  },
  {
    id: 2,
    actor: "Divya Nair",
    action: "approved",
    target: "SR-2026-1037",
    detail: "VPN access request",
    time: "1 hour ago",
  },
  {
    id: 3,
    actor: "Suresh Kumar",
    action: "resolved",
    target: "SR-2026-1038",
    detail: "Projector flickering",
    time: "2 hours ago",
  },
  {
    id: 4,
    actor: "Neha Gupta",
    action: "raised",
    target: "SR-2026-1039",
    detail: "Figma license request",
    time: "4 hours ago",
  },
  {
    id: 5,
    actor: "Meena Joshi",
    action: "closed",
    target: "SR-2026-1036",
    detail: "Desk chair replacement",
    time: "Yesterday",
  },
  {
    id: 6,
    actor: "Anita Desai",
    action: "commented on",
    target: "SR-2026-1035",
    detail: "Email delivery delays",
    time: "Yesterday",
  },
];

export const faqs = [
  {
    q: "How do I raise a new service request?",
    a: "Navigate to Service Requests → New Request, fill in the request type, department, priority, and a clear description of your issue, then submit. You'll receive a tracking number instantly.",
  },
  {
    q: "How long does it take to get a response?",
    a: "Response SLAs depend on priority: Critical — 1 hour, High — 4 hours, Medium — 1 business day, Low — 3 business days. You'll be notified when a technician is assigned.",
  },
  {
    q: "Who approves my requests?",
    a: "Requests that involve procurement, access, or budget are routed to your department HOD for approval before assignment. You can track approval status on the request detail page.",
  },
  {
    q: "Can I edit a request after submitting?",
    a: "Yes — open the request and click Edit. Note that editing a request that is already In Progress will notify the assigned technician of your changes.",
  },
  {
    q: "How do I escalate an urgent issue?",
    a: "Set the priority to Critical when raising the request, or add a comment mentioning @escalation on an existing request. The duty manager is alerted automatically.",
  },
  {
    q: "How is asset assignment tracked?",
    a: "Every asset has a unique tag. When IT assigns hardware to you, it appears under Assets → Assigned to me, along with warranty and support details.",
  },
];
